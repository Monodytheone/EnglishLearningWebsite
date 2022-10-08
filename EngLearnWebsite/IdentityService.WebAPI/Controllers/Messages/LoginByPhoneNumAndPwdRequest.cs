using FluentValidation;

namespace IdentityService.WebAPI.Controllers.Messages;

public record LoginByPhoneNumAndPwdRequest(string PhoneNumber, string Password);


public class LoginByPhoneNumAndPwdRequestValidator : AbstractValidator<LoginByPhoneNumAndPwdRequest>
{
    public LoginByPhoneNumAndPwdRequestValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().NotNull();
        RuleFor(x => x.Password).NotEmpty().NotNull();
    }
}
