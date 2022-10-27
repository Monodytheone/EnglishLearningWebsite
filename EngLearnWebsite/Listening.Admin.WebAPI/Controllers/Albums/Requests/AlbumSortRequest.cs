using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Listening.Admin.WebAPI.Controllers.Albums.Requests
{
    public record AlbumSortRequest([RequiredGuid]Guid CagetoryId, Guid[] SortedAlbumIds);

    public class AlbumSortRequestValidator : AbstractValidator<AlbumSortRequest>
    {
        public AlbumSortRequestValidator()
        {
            RuleFor(x => x.SortedAlbumIds).NotEmpty()
                .NotContains(Guid.Empty)
                .NotDuplicated().WithMessage("待排序id集合中有重复元素");
        }
    }
}
