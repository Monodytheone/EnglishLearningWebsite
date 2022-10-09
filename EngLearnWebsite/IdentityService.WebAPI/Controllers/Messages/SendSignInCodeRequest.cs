using FluentValidation;
using System.Text.RegularExpressions;

namespace IdentityService.WebAPI.Controllers.Messages
{
    public record SendSignInCodeRequest(string PhoneNumber);

    public class SendSignInCodeRequestValidator : AbstractValidator<SendSignInCodeRequest>
    {
        public SendSignInCodeRequestValidator()
        {
            RuleFor(e => e.PhoneNumber).NotNull().NotEmpty()
                .Length(1, 18)
                .Must(n => Regex.IsMatch(n, "^[0-9]+$"));
        }
    }
}
