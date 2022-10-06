
namespace FileService.Domain;

/// <summary>
/// 存储服务器接口
/// </summary>
public interface IStorageClient
{
    StorageType StorageType { get; }

    /// <summary>
    /// 保存文件
    /// </summary>
    /// <param name="key">文件的key（一般是文件路径的一部分）</param>
    /// <param name="content">文件内容</param>
    /// <returns>保存的文件的全路径（可以被访问的文件Url）</returns>
    Task<Uri> SaveAsync(string key, Stream content, CancellationToken cancellationToken = default);
}
