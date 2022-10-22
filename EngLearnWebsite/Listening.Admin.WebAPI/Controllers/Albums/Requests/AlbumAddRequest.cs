using FluentValidation;
using Listening.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Zack.DomainCommons.Models;

namespace Listening.Admin.WebAPI.Controllers.Albums.Requests
{
    public record AlbumAddRequest(Guid CategoryId, MultilingualString Name);

    public class AlbumAddRequestValidator : AbstractValidator<AlbumAddRequest>
    {
        public AlbumAddRequestValidator(ListeningDbContext dbContext)
        {
            RuleFor(x => x.CategoryId).NotEmpty().NotEqual(Guid.Empty)
                .MustAsync((cId, ct) => dbContext.Categories.AnyAsync(category => category.Id == cId)).WithMessage(c => $"CategoryId = {c.CategoryId}不存在");  // 异步的校验规则，只支持手动校验模式
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name.Chinese).NotEmpty().Length(1, 200);
            RuleFor(x => x.Name.English).NotNull().NotEmpty().Length(1, 200);
        }
    }
}
