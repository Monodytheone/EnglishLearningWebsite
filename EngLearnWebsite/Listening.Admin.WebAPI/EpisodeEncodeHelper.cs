using StackExchange.Redis;

namespace Listening.Admin.WebAPI;

public class EpisodeEncodeHelper
{
    private readonly IConnectionMultiplexer _redisConn;

    public EpisodeEncodeHelper(IConnectionMultiplexer redisConn)
    {
        _redisConn = redisConn;
    }

    /// <summary>
    /// 把待转码任务的详细信息存到Redis
    /// </summary>
    /// <param name="episode"></param>
    /// <returns></returns>
    public async Task AddEncodingEpisodeAsync(EncodingEpisodeInfo episode)
    {
        string redisKeyForEpisode = GetRedisKeyForEpisode(episode.Id);
        var db = _redisConn.GetDatabase();
        await db.StringSetAsync(redisKeyForEpisode, episode.ToJsonString());  // 保存转码任务详细信息到Redis
        // 这里有问题的，Subtitle.Content是Json形式时，意识episode.ToJsonString()看起来没问题，但存到redis中的那一项却会是null
        // 啊，不止json，srt也一样是null的，而且字幕类型对应的枚举也存成0了
        // 我决定暂时放弃处理这点，先让创建出的Episode的Subtitle.Content允许为null了

        Console.WriteLine($"{episode}\n{episode.ToJsonString()}");

        string keyOfAlbum = GetRedisKeyOfAlbum(episode.AlbumId);
        await db.StringSetAsync(keyOfAlbum, episode.Id.ToString());  // 将EpisodeId添加到<album下所有的待转码episodeId>中
    }

    public async Task<EncodingEpisodeInfo> GetEncodingEpisodeAsync(Guid episodeId)
    {
        string episodeKey = GetRedisKeyForEpisode(episodeId);
        var db = _redisConn.GetDatabase();
        string json = await db.StringGetAsync(episodeKey);
        return json.ParseJson<EncodingEpisodeInfo>()!;
    }


    public async Task<IEnumerable<Guid>> GetEncodingEpisodeIdsByAlbumIdAsync(Guid albumId)
    {
        string key = GetRedisKeyOfAlbum(albumId);
        var db = _redisConn.GetDatabase();
        var values = await db.SetMembersAsync(key);  // 这个方法名我硬是没搞懂啥意思，大概Set是集合的意思？
        return values.Select(value => Guid.Parse(value));
    }

    /// <summary>
    /// 修改Episode的转码状态
    /// </summary>
    /// <param name="episodeId"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public async Task UpdateEpisodeEncodingStatusAsync(Guid episodeId, EncodeStatus status)
    {
        string episodeKey = EpisodeEncodeHelper.GetRedisKeyForEpisode(episodeId);
        var db = _redisConn.GetDatabase();
        string? originalJson = await db.StringGetAsync(episodeKey);

        if (originalJson == null)
        {
            Console.WriteLine("可能收到了之前运行中发出的集成事件");
            return;
        }

        EncodingEpisodeInfo encodingEpisodeInfo = originalJson.ParseJson<EncodingEpisodeInfo>()!;
        encodingEpisodeInfo = encodingEpisodeInfo with { Status = status };
        await db.StringSetAsync(episodeKey, encodingEpisodeInfo.ToJsonString());
    }

    private static string GetRedisKeyForEpisode(Guid episodeId)
    {
        string redisKey = $"Listening.EncodingEpisode.{episodeId}";
        return redisKey;
    }

    private static string GetRedisKeyOfAlbum(Guid albumId)
    {
        return $"Listening.EncodingEpisodeIdsOfAlbum.{albumId}";
    }
}
