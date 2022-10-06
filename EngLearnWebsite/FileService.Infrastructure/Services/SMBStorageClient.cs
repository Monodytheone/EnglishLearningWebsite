using FileService.Domain;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Infrastructure.Services
{
    /// <summary>
    /// 用局域网内共享文件夹或者本机磁盘当备份服务器的实现类
    /// </summary>
    public class SMBStorageClient : IStorageClient
    {
        private readonly IOptionsSnapshot<SMBStorageOptions> _smbOptions;

        public SMBStorageClient(IOptionsSnapshot<SMBStorageOptions> smbOptions)
        {
            _smbOptions = smbOptions;
        }

        public StorageType StorageType => StorageType.BackUp;

        public async Task<Uri> SaveAsync(string key, Stream content, CancellationToken cancellationToken = default)
        {
            if (key.StartsWith('/'))
            {
                throw new ArgumentException("key shouldn't start with '/'", nameof(key));
            }
            string workingDir = _smbOptions.Value.WorkingDir;
            string fullPath = Path.Combine(workingDir, key);  // 拼出上传项存储的完整路径
            string? fullDir = Path.GetDirectoryName(fullPath);
            if (Directory.Exists(fullDir) == false)
            {
                Directory.CreateDirectory(fullDir);
            }
            if (File.Exists(fullPath) == true)  // 如果文件已存在，则尝试删除
            {
                File.Delete(fullPath);
            }
            using Stream outStream = File.OpenWrite(fullPath);
            await content.CopyToAsync(outStream, cancellationToken);
            return new Uri(fullPath);
        }
    }
}
