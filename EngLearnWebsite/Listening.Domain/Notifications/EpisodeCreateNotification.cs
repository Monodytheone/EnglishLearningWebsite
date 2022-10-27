using Listening.Domain.Entities;
using MediatR;

namespace Listening.Domain.Notifications
{
    public record EpisodeCreateNotification(Episode CreatedEpisode) : INotification;
}
