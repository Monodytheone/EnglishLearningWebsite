using FluentValidation;
using System.Text.RegularExpressions;

namespace IdentityService.WebAPI.Controllers.Messages
{
    public record LoginByPhoneNumAndSmsCodeRequest(string PhoneNumber, string Code);

    public class LoginByPhoneNumAndSmsCodeRequestValidator : AbstractValidator<LoginByPhoneNumAndSmsCodeRequest>
    {
        public LoginByPhoneNumAndSmsCodeRequestValidator()
        {
            RuleFor(r => r.PhoneNumber)
                .NotEmpty().NotNull()
                .Length(1, 18)
                .Must(phoneNum => Regex.IsMatch(phoneNum, "^[0-9]+$"));
            RuleFor(r => r.Code)
                .NotEmpty().NotNull()
                .Length(1, 4)
                .Must(code => Regex.IsMatch(code, "^[0-9]+$"));
        }
    }
}
