using FluentValidation;

namespace Listening.Admin.WebAPI.Controllers.Categories.Requests; 

public class CategoriesSortRequest
{
    /// <summary>
    /// 排好序的Id
    /// </summary>
    public Guid[] SortedCategoryIds { get; set; }
}

public class CategoriesSortRequestValidator : AbstractValidator<CategoriesSortRequest>
{
    public CategoriesSortRequestValidator()
    {
        RuleFor(c => c.SortedCategoryIds).NotEmpty().NotContains(Guid.Empty)
            .NotDuplicated().WithMessage("集合中有重复元素");  // 集合中没有重复元素
    }
}
