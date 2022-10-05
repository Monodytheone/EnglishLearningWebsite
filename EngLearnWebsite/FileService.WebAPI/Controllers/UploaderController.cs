using FileService.Domain;
using FileService.Domain.Entities;
using FileService.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Zack.ASPNETCore;

namespace FileService.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[UnitOfWork(typeof(FSDbContext))]
public class UploaderController : ControllerBase
{
    private readonly IFSRepository _fsRepository;
    private readonly FSDomainService _fsDomainService;
    private readonly FSDbContext _fsDbContext;

    public UploaderController(IFSRepository fsRepository, FSDomainService fsDomainService, FSDbContext fsDbContext)
    {
        _fsRepository = fsRepository;
        _fsDomainService = fsDomainService;
        _fsDbContext = fsDbContext;
    }

    [HttpGet]
    public async Task<FileExistResponse> FileExists(long fileSize, string sha256Hash)
    {
        UploadedItem? existedItem = await _fsRepository.FindFileAsync(fileSize, sha256Hash);
        if (existedItem == null)
        {
            return new FileExistResponse(false, null);
        }
        else
        {
            return new FileExistResponse(true, existedItem.RemoteUrl);
        }
    }

    [HttpPost]
    [RequestSizeLimit(60_000_000)]
    public async Task<ActionResult<Uri>> Upload([FromForm] UploadRequest request, CancellationToken cancellationToken = default)
    {
        IFormFile file = request.File;
        string fileName = file.FileName;
        using Stream stream = file.OpenReadStream();
        UploadedItem uploadItem = await _fsDomainService.UploadAsync(stream, fileName, cancellationToken);
        _fsDbContext.UploadedItems.Add(uploadItem);
        return Ok(uploadItem.RemoteUrl);
    }
}
