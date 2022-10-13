using FluentValidation;
using System.Text.RegularExpressions;

namespace IdentityService.WebAPI.Controllers.Messages
{
    public record EditAdminUserRequest  (string PhoneNumber);

    public class EditAdminuserRequestValidator : AbstractValidator<EditAdminUserRequest>
    {
        public EditAdminuserRequestValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().NotNull()
                .Length(1, 18)
                .Must(pn => Regex.IsMatch(pn, "^[0-9]+$"));
        }
    }
}
