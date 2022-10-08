using FluentValidation;

namespace IdentityService.WebAPI.Controllers.Messages
{
    public record ChangeMyPasswordRequest(string NewPassword);

    public class ChangeMyPasswordRequestValidator : AbstractValidator<ChangeMyPasswordRequest>
    {
        public ChangeMyPasswordRequestValidator()
        {
            RuleFor(x => x.NewPassword).NotEmpty().NotNull().Length(6, 18);
        }
    }
}
