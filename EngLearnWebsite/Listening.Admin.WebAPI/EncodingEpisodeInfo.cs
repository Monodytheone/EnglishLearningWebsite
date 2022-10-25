using Listening.Domain.Entities.ValueObjects;
using Zack.DomainCommons.Models;

namespace Listening.Admin.WebAPI;

/// <summary>
/// 转码状态
/// </summary>
public enum EncodeStatus
{
    Created,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed,
}

public record EncodingEpisodeInfo(Guid Id, MultilingualString Name, Guid AlbumId, double DurationInSecond, Subtitle Subtitle, EncodeStatus Status);
