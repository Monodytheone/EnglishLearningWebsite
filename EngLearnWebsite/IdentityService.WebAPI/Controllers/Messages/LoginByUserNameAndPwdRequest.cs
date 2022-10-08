using FluentValidation;

namespace IdentityService.WebAPI.Controllers.Messages;

public record LoginByUserNameAndPwdRequest(string UserName, string Password);

public class LoginByUserNameAndPwdRequestValidator : AbstractValidator<LoginByUserNameAndPwdRequest>
{
    public LoginByUserNameAndPwdRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().NotNull();
        RuleFor(x => x.Password).NotEmpty().NotNull(); 
    }
}
