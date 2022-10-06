
using Zack.DomainCommons.Models;

namespace FileService.Domain.Entities;

/// <summary>
/// 上传（后的）项
/// </summary>
public record UploadedItem : BaseEntity, IHasCreationTime
{
    public DateTime CreationTime { get; private set; }

    /// <summary>
    /// 以字节为单位的文件大小
    /// </summary>
    public long FileSizeInBytes { get; private set; }

    /// <summary>
    /// 用户上传的原始文件名（不包含路径）
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// 文件内容的哈希值（采用SHA-256算法）
    /// </summary>
    /// <remarks>两个文件的大小和散列值（SHA256）都相同的概率非常小。
    /// 因此只要大小和SHA256相同，就认为是相同的文件。</remarks>
    public string FileSHA256Hash { get; private set; }

    /// <summary>
    /// 备份服务器中文件的路径
    /// </summary>
    public Uri BackupUrl { get; private set; }

    /// <summary>
    /// 文件服务器中文件的路径
    /// </summary>
    /// <remarks>上传的文件的供外部访问者访问的路径</remarks>
    public Uri RemoteUrl { get; private set; }

    public static UploadedItem Create(Guid id, long fileSizeInBytes, string fileName, string fileSHA256Hash, Uri backupUrl, Uri remoteUrl)
    {
        UploadedItem item = new UploadedItem()
        {
            Id = id,
            CreationTime = DateTime.Now,
            FileName = fileName,
            FileSizeInBytes = fileSizeInBytes,
            FileSHA256Hash = fileSHA256Hash,
            BackupUrl = backupUrl,
            RemoteUrl = remoteUrl,
        };
        return item;
    }

}
