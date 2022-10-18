using Listening.Domain.Entities;
using MediatR;

namespace Listening.Domain.Notifications;

public record EpisodeUpdateNotification(Episode UpdatedEpisode) : INotification;
