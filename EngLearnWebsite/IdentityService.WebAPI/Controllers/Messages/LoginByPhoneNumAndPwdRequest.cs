using FluentValidation;
using System.Text.RegularExpressions;

namespace IdentityService.WebAPI.Controllers.Messages;

public record LoginByPhoneNumAndPwdRequest(string PhoneNumber, string Password);


public class LoginByPhoneNumAndPwdRequestValidator : AbstractValidator<LoginByPhoneNumAndPwdRequest>
{
    public LoginByPhoneNumAndPwdRequestValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().NotNull()
            .Length(1, 18)
            //.Must(n => Regex.IsMatch(n, "[0-9]+"));
            .Must(n => Regex.IsMatch(n, "^[0-9]+$"));
        RuleFor(x => x.Password).NotEmpty().NotNull();
    }
}
