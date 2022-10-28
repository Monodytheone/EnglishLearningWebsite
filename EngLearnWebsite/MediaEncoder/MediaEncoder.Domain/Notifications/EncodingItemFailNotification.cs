using MediatR;

namespace MediaEncoder.Domain.Notifications;

public record EncodingItemFailNotification(Guid EncodingItemId, string SourceSystem, string ErrorMessage)
    : INotification;
