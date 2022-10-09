using MediatR;

namespace IdentityService.WebAPI.MediatREvents
{
    public record UserCreatedEvent(string PhoneNumber, string Password) : INotification;
}
