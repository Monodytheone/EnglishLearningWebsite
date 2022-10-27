using Listening.Domain.Notifications;
using MediatR;
using Zack.EventBus;

namespace Listening.Admin.WebAPI.NotificationHandlers;

public class EpisodeSoftDeleteNotificationHandler : INotificationHandler<EpisodeSoftDeleteNotification>
{
    private readonly IEventBus _eventBus;

    public EpisodeSoftDeleteNotificationHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task Handle(EpisodeSoftDeleteNotification notification, CancellationToken cancellationToken)
    {
        _eventBus.Publish("Listening.Episode.SoftDelete", new { Id = notification.EpisodeId });
        return Task.CompletedTask;
    }
}
