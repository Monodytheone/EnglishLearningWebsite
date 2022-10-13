using IdentityService.Domain;
using MediatR;

namespace IdentityService.WebAPI.MediatREvents
{
    public class PasswordResetEventHandler : INotificationHandler<PasswordResetEvent>
    {
        private readonly ISmsSender _smsSender;

        public PasswordResetEventHandler(ISmsSender smsSender)
        {
            _smsSender = smsSender;
        }

        public async Task Handle(PasswordResetEvent notification, CancellationToken cancellationToken)
        {
            string msg = $"用户 {notification.UserName} 的密码已重设为 {notification.Password}\n";
            await _smsSender.SendAsync(notification.PhoneNumber, msg);
        }
    }
}
