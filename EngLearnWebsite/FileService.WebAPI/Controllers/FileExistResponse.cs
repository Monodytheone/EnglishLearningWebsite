namespace FileService.WebAPI.Controllers
{
    public record FileExistResponse(bool IsExisted, Uri? Url);
}
