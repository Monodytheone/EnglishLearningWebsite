using Nest;
using SearchService.Domain;

namespace SearchService.Infrastructure;

public class SearchRepository : ISearchRepository
{
    private readonly IElasticClient _elasticClient;

    public SearchRepository(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public Task DeleteAsync(Guid episodeId)
    {
        _elasticClient.DeleteByQuery<EpisodeInfo>(q => q.Index("episodes").Query(rq => rq.Term(f => f.Id, "elasticsearch.pm")));
        // 因为有可能文档不存在，所以不检查结果
        // 如果Episode被删除，则把对应的数据也从Elastic Search中删除
        return _elasticClient.DeleteAsync(new DeleteRequest("episodes", episodeId));
    }

    public async Task<SearchEpisodeInfosResponse> SearchEpisodeInfosAsync(string keyword, int pageIndex, int pageSize)
    {
        int from = pageSize * (pageIndex - 1);
        Func<QueryContainerDescriptor<EpisodeInfo>, QueryContainer> query = (q) =>
                q.Match(mq => mq.Field(f => f.CnName).Query(keyword))
                || q.Match(mq => mq.Field(f => f.EngName).Query(keyword))
                || q.Match(mq => mq.Field(f => f.PlainSubtitle).Query(keyword));
        Func<HighlightDescriptor<EpisodeInfo>, IHighlight> highlightSelector = h =>
                h.Fields(fs => fs.Field(f => f.PlainSubtitle));
        var result = await _elasticClient.SearchAsync<EpisodeInfo>(s => s.Index("episodes").From(from)
            .Size(pageSize).Query(query).Highlight(highlightSelector));
        if (result.IsValid == false)
        {
            throw result.OriginalException;
        }

        List<EpisodeInfo> episodeInfos = new();
        foreach (var hit in result.Hits)
        {
            string highlightedSubtitle;
            //如果没有预览内容，则显示前50个字
            if (hit.Highlight.ContainsKey("plainSubtitle"))
            {
                highlightedSubtitle = string.Join("\r\n", hit.Highlight["plainSubtitle"]);
            }
            else
            {
                highlightedSubtitle = hit.Source.PlainSubtitle.Cut(50);
            }
            EpisodeInfo episodeInfo = hit.Source with { PlainSubtitle = highlightedSubtitle };
            episodeInfos.Add(episodeInfo);
        }
        return new SearchEpisodeInfosResponse(episodeInfos, result.Total);
    }

    public async Task UpsertAsync(EpisodeInfo episodeInfo)
    {
        IndexResponse response = await _elasticClient.IndexAsync(episodeInfo, idx => idx.Index("episodes").Id(episodeInfo.Id));
        if (response.IsValid == false)
        {
            throw new ApplicationException(response.DebugInformation);
        }
    }
}
