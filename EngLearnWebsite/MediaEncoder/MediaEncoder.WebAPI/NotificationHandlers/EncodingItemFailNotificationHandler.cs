using MediaEncoder.Domain.Notifications;
using MediatR;
using Zack.EventBus;

namespace MediaEncoder.WebAPI.NotificationHandlers;

internal class EncodingItemFailNotificationHandler : INotificationHandler<EncodingItemFailNotification>
{
    private readonly IEventBus _eventBus;

    public EncodingItemFailNotificationHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task Handle(EncodingItemFailNotification notification, CancellationToken cancellationToken)
    {
        _eventBus.Publish("MediaEncoding.Failed", notification);
        return Task.CompletedTask;
    }
}
