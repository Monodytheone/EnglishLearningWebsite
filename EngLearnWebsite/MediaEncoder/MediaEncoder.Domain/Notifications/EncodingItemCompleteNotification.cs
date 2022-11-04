using MediatR;

namespace MediaEncoder.Domain.Notifications;

public record EncodingItemCompleteNotification(Guid EncodingItemId, string SourceSystem, Uri OutputUrl)
    : INotification;
