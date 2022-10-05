
using FileService.Domain.Entities;

namespace FileService.Domain;

public interface IFSRepository
{
    /// <summary>
    /// 根据文件大小和哈希值查找上传记录
    /// </summary>
    Task<UploadedItem?> FindFileAsync(long fileSize, string sha256Hash);
}
