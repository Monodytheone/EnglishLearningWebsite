using FluentValidation;
using Listening.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Listening.Admin.WebAPI.Controllers.Episodes.Requests;

public record EpisodeSortRequest([RequiredGuid] Guid AlbumId, Guid[] SortedEpisodeIds);

public class EpisodeSortRequestValidator : AbstractValidator<EpisodeSortRequest>
{
	public EpisodeSortRequestValidator(ListeningDbContext dbContext)
	{
		RuleFor(x => x.AlbumId).MustAsync((albumId, ct) => dbContext.Albums.AnyAsync(album => album.Id == albumId));
		RuleFor(x => x.SortedEpisodeIds).NotEmpty().NotContains(Guid.Empty)
			.NotDuplicated().WithMessage("SortedEpisodeId集合中有重复元素");
	}
}