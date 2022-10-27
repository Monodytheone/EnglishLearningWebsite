using FluentValidation;
using Listening.Domain.Entities.ValueObjects;
using Listening.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Zack.DomainCommons.Models;

namespace Listening.Admin.WebAPI.Controllers.Episodes.Requests;

public record EpisodeAddRequest(MultilingualString Name, [RequiredGuid]Guid AlbumId, Uri AudioUrl, double DurationInSecond, string Subtitle_Content, string Subtitle_Format);

public class EpisodeAddRequestValidator : AbstractValidator<EpisodeAddRequest>
{
    public EpisodeAddRequestValidator(ListeningDbContext dbContext)
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Name.Chinese).NotEmpty().Length(1, 200);
        RuleFor(x => x.Name.English).NotEmpty().Length(1, 200);
        RuleFor(x => x.AlbumId).MustAsync(
            (albumId, cToken) => dbContext.Albums.AnyAsync(album => album.Id == albumId)
            ).WithMessage("albumId不存在");
        RuleFor(x => x.AudioUrl).NotEmptyUri().Length(1, 1000);
        RuleFor(x => x.DurationInSecond).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Subtitle_Content).NotEmpty();
        RuleFor(x => x.Subtitle_Format).IsEnumName(typeof(SubtitleType)).WithMessage(x => $"'Subtitle_ Format' 的值范围不包含 '{x.Subtitle_Format}' （导致本错误的原因可能是未区分大小写）'");
    }
}
