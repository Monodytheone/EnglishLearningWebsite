using FluentValidation;

namespace SearchService.WebAPI.Controllers.Requests;

public record SearchEpisodesRequest(string Keyword, int PageIndex, int PageSize);

public class SearchEpisodesRequestValidator : AbstractValidator<SearchEpisodesRequest>
{
    public SearchEpisodesRequestValidator()
    {
        RuleFor(x => x.Keyword).NotNull().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.PageIndex).GreaterThan(0);  // 页号从1开始
        RuleFor(x => x.PageSize).GreaterThanOrEqualTo(5);
    }
}
