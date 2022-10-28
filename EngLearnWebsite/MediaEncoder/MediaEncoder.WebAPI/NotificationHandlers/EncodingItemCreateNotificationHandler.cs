using MediaEncoder.Domain.Notifications;
using MediatR;
using Zack.EventBus;

namespace MediaEncoder.WebAPI.NotificationHandlers;

internal class EncodingItemCreateNotificationHandler : INotificationHandler<EncodingItemCreateNotification>
{
    private readonly IEventBus _eventBus;

    public EncodingItemCreateNotificationHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task Handle(EncodingItemCreateNotification notification, CancellationToken cancellationToken)
    {
        // 啥也没干哈哈哈，EncodingItem的Create方法只在MediaEncodingCreatedHandler中调用，创建后就插入数据库等待托管服务来转码了，目前还不需要再通知听力服务知道
        return Task.CompletedTask;
    }
}
