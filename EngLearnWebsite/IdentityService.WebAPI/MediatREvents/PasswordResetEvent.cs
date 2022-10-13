using MediatR;

namespace IdentityService.WebAPI.MediatREvents;

public record PasswordResetEvent(string PhoneNumber, string UserName, string Password) : INotification;
