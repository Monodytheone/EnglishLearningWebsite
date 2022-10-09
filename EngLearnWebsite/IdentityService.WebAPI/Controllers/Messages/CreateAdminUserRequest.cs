using FluentValidation;
using System.Text.RegularExpressions;

namespace IdentityService.WebAPI.Controllers.Messages
{
    public record CreateAdminUserRequest(string PhoneNumber, string UserName);

    public class CreateAdminUserRequestValidator : AbstractValidator<CreateAdminUserRequest>
    {
        public CreateAdminUserRequestValidator()
        {
            RuleFor(r => r.PhoneNumber)
                .NotEmpty().NotNull()
                .Length(1, 18)
                .Must(pn => Regex.IsMatch(pn, "^[0-9]+$")).WithMessage("手机号只能由数字组成");
            RuleFor(r => r.UserName)
                .NotEmpty().NotNull()
                .Length(2, 20);
        }
    }
}
