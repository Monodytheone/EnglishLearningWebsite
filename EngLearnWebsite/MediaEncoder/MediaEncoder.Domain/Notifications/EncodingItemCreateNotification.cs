using MediaEncoder.Domain.Entities;
using MediatR;

namespace MediaEncoder.Domain.Notifications;

public record EncodingItemCreateNotification(EncodingItem EncodingItem) : INotification;
