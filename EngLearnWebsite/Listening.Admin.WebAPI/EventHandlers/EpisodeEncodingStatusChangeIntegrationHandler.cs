using Listening.Admin.WebAPI.Hubs;
using Listening.Domain;
using Listening.Domain.Entities;
using Listening.Domain.Entities.ValueObjects;
using Listening.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Zack.EventBus;

namespace Listening.Admin.WebAPI.EventHandlers;

/// <summary>
/// 收听转码服务发出的集成事件
/// 把状态通过SignalR推送给客户端，从而显示“转码进度”
/// </summary>
[EventName("MediaEncoding.Started")]
[EventName("MediaEncoding.Failed")]
[EventName("MediaEncoding.Duplicated")]
[EventName("MediaEncoding.Completed")]
public class EpisodeEncodingStatusChangeIntegrationHandler : DynamicIntegrationEventHandler
{
    private readonly IHubContext<EpisodeEncodingStatusHub> _hubContext;
    private readonly EpisodeEncodeHelper _episodeEncodeHelper;
    private readonly IListeningRepository _repository;
    private readonly ListeningDbContext _dbContext;

    public EpisodeEncodingStatusChangeIntegrationHandler(IHubContext<EpisodeEncodingStatusHub> hubContext, EpisodeEncodeHelper episodeEncodeHelper, IListeningRepository repository, ListeningDbContext dbContext)
    {
        _hubContext = hubContext;
        _episodeEncodeHelper = episodeEncodeHelper;
        _repository = repository;
        _dbContext = dbContext;
    }

    public override async Task HandleDynamic(string eventName, dynamic eventData)
    {
        string sourceSystem = eventData.SourceSystem;
        if (sourceSystem != "Listening")  // 可能是别的系统的转码消息
        {
            return;
        }

        Guid episodeId = Guid.Parse(eventData.EncodingItemId);
        switch (eventName)
        {
            case "MediaEncoding.Started":
                await _episodeEncodeHelper.UpdateEpisodeEncodingStatusAsync(episodeId, EncodeStatus.Started);
                await _hubContext.Clients.All.SendAsync("OnMediaEncodingStarted", episodeId);  // 通知前端刷新
                break;
            case "MediaEncoding.Failed":
                await _episodeEncodeHelper.UpdateEpisodeEncodingStatusAsync(episodeId, EncodeStatus.Failed);
                //todo: 这样做有问题，这样就会把消息发送给所有打开这个界面的人，应该用connectionId、userId等进行过滤，
                await _hubContext.Clients.All.SendAsync("OnMediaEncodingFailed", episodeId);
                break;
            case "MediaEncoding.Duplicated":
                await _episodeEncodeHelper.UpdateEpisodeEncodingStatusAsync(episodeId, EncodeStatus.Completed);
                await _hubContext.Clients.All.SendAsync("OnMediaEncodingCompleted", episodeId);
                break;
            case "MediaEncoding.Completed":
                Console.WriteLine("听力服务收到转码完成消息");
                Console.WriteLine(eventData);

                // 转码完成，则从Redis中把暂存的Episode信息取出来，然后正式地插入Episode表中

                try
                {
                    await _episodeEncodeHelper.UpdateEpisodeEncodingStatusAsync(episodeId, EncodeStatus.Completed);
                    Uri outputUrl = new Uri(eventData.OutputUrl);
                    EncodingEpisodeInfo encodeItem = await _episodeEncodeHelper.GetEncodingEpisodeAsync(episodeId);

                    Guid albumId = encodeItem.AlbumId;
                    int sequence = await _repository.GetMaxSeqOfEpisodeAsync(albumId) + 1;

                    // 妥协：随便搞个Content不为空的Subtitle插进数据库先
                    Subtitle subtitle = null;
                    if (encodeItem.Subtitle.Content != null)
                    {
                        subtitle = encodeItem.Subtitle;
                    }
                    else
                    {
                        subtitle = new("[{\"StartTime\":\"00:00:00\",\"EndTime\":\"00:00:01\",\"Value\":\"字幕处理失败了哦\"}]", SubtitleType.Json);
                    }

                    Episode episode = new Episode.Builder()
                        .Id(episodeId).SequenceNumber(sequence).Name(encodeItem.Name).AlbumId(albumId)
                        .AudioUrl(outputUrl).DurationInSecond(encodeItem.DurationInSecond)
                        .Subtitle(subtitle)
                        .Build();
                    _dbContext.Episodes.Add(episode);
                    await _dbContext.SaveChangesAsync();

                    await _hubContext.Clients.All.SendAsync("OnMediaEncodingCompleted", episodeId);  // 通知前端刷新
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(eventName));
        }
    }
}
