using MediaEncoder.Domain.Entities;
using MediaEncoder.Infrastructure;
using Zack.EventBus;

namespace MediaEncoder.WebAPI.EventHandlers
{
    /// <summary>
    /// 接收Listening.Admin的EpisodeController中的Add方法发出的转码事件。
    /// 新建转码任务，插入到数据库，供托管服务取过来进行转码
    /// </summary>
    [EventName("MediaEncoding.Created")]
    public class MediaEncodingCreateEventHandler : DynamicIntegrationEventHandler
    {
        private readonly MEDbContext _dbContext;

        public MediaEncodingCreateEventHandler(MEDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task HandleDynamic(string eventName, dynamic eventData)
        {
            
            Guid Id = eventData.MediaId;
            //Uri sourceUrl = eventData.MediaUrl;
            Uri sourceUrl = new Uri(eventData.MediaUrl);
            string name = sourceUrl.Segments.Last();
            string destFormatString = eventData.OutPutFormat;
            MediaFormat destFormat = Enum.Parse<MediaFormat>(destFormatString.ToLower());
            string sourceSystem = eventData.SourceSystem;

            bool encodingTaskExisting = _dbContext.EncodingItems.Any(item => item.SourceUrl == sourceUrl && item.DestFormat == destFormat);
            if (encodingTaskExisting)  // 如果相同的转码任务已存在，则返回
            {
                return;
            }

            // 直接用对应的EpisodeId作为EncodingItem的Id
            var newEncodingItem = EncodingItem.Create(Id, name, sourceUrl, destFormat, sourceSystem);
            _dbContext.EncodingItems.Add(newEncodingItem);
            await _dbContext.SaveChangesAsync();
        }
    }
}
