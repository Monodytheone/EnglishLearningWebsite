using MediaEncoder.Domain.Notifications;
using MediatR;
using Zack.EventBus;

namespace MediaEncoder.WebAPI.NotificationHandlers;

internal class EncodingItemCompleteNotificationHandler : INotificationHandler<EncodingItemCompleteNotification>
{
    private readonly IEventBus _eventBus;

    public EncodingItemCompleteNotificationHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task Handle(EncodingItemCompleteNotification notification, CancellationToken cancellationToken)
    {
        _eventBus.Publish("MediaEncoding.Completed", notification);
        return Task.CompletedTask;
    }
}
