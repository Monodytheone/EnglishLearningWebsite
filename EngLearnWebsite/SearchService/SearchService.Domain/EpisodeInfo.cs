namespace SearchService.Domain;

public record EpisodeInfo(Guid Id, string CnName, string EngName, string PlainSubtitle, Guid AlbumId);
