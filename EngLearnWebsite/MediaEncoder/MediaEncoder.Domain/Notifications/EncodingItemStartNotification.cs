using MediatR;

namespace MediaEncoder.Domain.Notifications;

public record EncodingItemStartNotification(Guid EncodingItemId, string SourceSystem) : INotification;
