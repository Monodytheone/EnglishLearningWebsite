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
