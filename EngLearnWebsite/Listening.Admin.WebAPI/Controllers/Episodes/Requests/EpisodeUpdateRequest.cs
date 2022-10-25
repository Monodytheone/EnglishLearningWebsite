using FluentValidation;
using Listening.Domain.Entities.ValueObjects;
using Zack.DomainCommons.Models;

namespace Listening.Admin.WebAPI.Controllers.Episodes.Requests;

// Episode的音频不能修改，否则会让代码复杂很多，主流视频网站也都是这样干的。
public record EpisodeUpdateRequest(MultilingualString Name, string Subtitle_Content, string Subtitle_Format);

public class EpisodeUpdateRequestValidator : AbstractValidator<EpisodeUpdateRequest>
{
    public EpisodeUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Name.Chinese).NotEmpty().Length(1, 200);
        RuleFor(x => x.Name.English).NotEmpty().Length(1, 200);
        RuleFor(x => x.Subtitle_Content).NotEmpty();
        RuleFor(x => x.Subtitle_Format).Length(1, 10)
            .IsEnumName(typeof(SubtitleType)).WithMessage(x => $"'Subtitle_ Format' 的值范围不包含 '{x.Subtitle_Format}' （导致本错误的原因可能是未区分大小写）'");
    }
}
