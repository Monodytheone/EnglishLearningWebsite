using FluentValidation;
using Zack.DomainCommons.Models;

namespace Listening.Admin.WebAPI.Controllers.Categories.Requests
{
    public record CategoryAddRequest(MultilingualString Name, Uri CoverUrl);

    public class CategoryAddRequestValidator : AbstractValidator<CategoryAddRequest>
    {
        public CategoryAddRequestValidator()
        {
            RuleFor(x => x.Name).NotNull();
            RuleFor(x => x.Name.English).NotNull().Length(1, 200);
            RuleFor(x => x.Name.Chinese).NotNull().Length(1, 200);
            RuleFor(x => x.CoverUrl).Length(5, 500);  // CoverUrl允许为空
        }
    }
}
