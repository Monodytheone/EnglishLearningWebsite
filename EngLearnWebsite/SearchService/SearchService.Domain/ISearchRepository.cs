namespace SearchService.Domain;

public interface ISearchRepository
{
    /// <summary>
    ///  更新或插入
    /// </summary>
    public Task UpsertAsync(EpisodeInfo episodeInfo);

    public Task DeleteAsync(Guid episodeId);

    public Task<SearchEpisodeInfosResponse> SearchEpisodeInfosAsync(string keyword, int pageIndex, int pageSize);
}
