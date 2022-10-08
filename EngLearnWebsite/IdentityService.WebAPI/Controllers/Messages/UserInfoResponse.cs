namespace IdentityService.WebAPI.Controllers.Messages
{
    public record UserInfoResponse(Guid Id, string PhoneNumber, DateTime CreationTime);
}
