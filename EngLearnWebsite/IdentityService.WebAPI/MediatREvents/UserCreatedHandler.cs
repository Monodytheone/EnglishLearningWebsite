using IdentityService.Domain;
using MediatR;

namespace IdentityService.WebAPI.MediatREvents
{
    public class UserCreatedHandler : INotificationHandler<UserCreatedEvent>
    {
        private readonly ISmsSender _smsSender;

        public UserCreatedHandler(ISmsSender smsSender)
        {
            _smsSender = smsSender;
        }

        public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
        {
            await _smsSender.SendAsync(notification.PhoneNumber, $"手机号为 {notification.PhoneNumber}的用户，你的密码是 {notification.Password}.");
        }
    }
}
