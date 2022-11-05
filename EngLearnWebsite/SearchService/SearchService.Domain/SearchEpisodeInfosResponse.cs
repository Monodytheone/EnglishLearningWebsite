namespace SearchService.Domain;

public record SearchEpisodeInfosResponse(IEnumerable<EpisodeInfo> EpisodeInfos, long TotalCount);
