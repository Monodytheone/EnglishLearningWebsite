using FluentValidation;
using Zack.DomainCommons.Models;

namespace Listening.Admin.WebAPI.Controllers.Albums.Requests
{
    public record AlbumUpdateRequest(MultilingualString Name);

    public class AlbumUpdateRequestValidator : AbstractValidator<AlbumUpdateRequest>
    {
        public AlbumUpdateRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Name.Chinese).NotEmpty().Length(1, 200);
            RuleFor(x=> x.Name.English).NotEmpty().Length(1, 200);
        }
    }
}
