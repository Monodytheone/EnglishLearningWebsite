using MediaEncoder.Domain.Notifications;
using MediatR;
using Zack.EventBus;

namespace MediaEncoder.WebAPI.NotificationHandlers;

internal class EncodingItemStartNotificationHandler : INotificationHandler<EncodingItemStartNotification>
{
    private readonly IEventBus _eventBus;

    public EncodingItemStartNotificationHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task Handle(EncodingItemStartNotification notification, CancellationToken cancellationToken)
    {
        // 把转码任务状态变化的领域事件，转换为集成事件发出
        _eventBus.Publish("MediaEncoding.Started", notification);
        return Task.CompletedTask;
    }
}
