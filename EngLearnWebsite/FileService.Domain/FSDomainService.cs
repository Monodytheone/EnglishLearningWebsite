
using FileService.Domain.Entities;
using Zack.Commons;

namespace FileService.Domain;

public class FSDomainService
{
    private readonly IFSRepository _repository;
    /// <summary>
    /// 备份服务器
    /// </summary>
    private readonly IStorageClient _backupStorage;
    /// <summary>
    /// 文件存储服务器
    /// </summary>
    private readonly IStorageClient _remoteStorage;

    // 这就用.net内置的依赖注入框架实现了按照条件注入的效果
    public FSDomainService(IFSRepository fsRepository, IEnumerable<IStorageClient> storageClients)
    {
        _repository = fsRepository;
        _backupStorage = storageClients.First(client => client.StorageType == StorageType.BackUp);
        _remoteStorage = storageClients.First(client => client.StorageType == StorageType.Public);
    }   

    public async Task<UploadedItem> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken)
    {
        string hash = HashHelper.ComputeSha256Hash(stream);
        long fileSize = stream.Length; 
        DateTime today = DateTime.Today;
        string key = $"{today.Year}/{today.Month}/{today.Day}/{hash}/{fileName}";

        UploadedItem? oldUploadItem = await _repository.FindFileAsync(fileSize, hash);
        if (oldUploadItem != null)  // 如果相同的文件已上传过，则返回旧上传项
        {
            return oldUploadItem;
        }

        Uri backupUrl = await _backupStorage.SaveAsync(key, stream, cancellationToken);  // 保存到本地备份
        stream.Position = 0;
        Uri remoteUrl = await _remoteStorage.SaveAsync(key, stream, cancellationToken); // 保存到生产的存储系统
        stream.Position = 0;

        Guid id = Guid.NewGuid();
        UploadedItem newUploadItem = UploadedItem.Create(id, fileSize, fileName, hash, backupUrl, remoteUrl);
        return newUploadItem;
    }
}
