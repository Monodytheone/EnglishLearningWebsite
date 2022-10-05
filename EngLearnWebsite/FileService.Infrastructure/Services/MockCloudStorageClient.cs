
using FileService.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FileService.Infrastructure.Services
{
    /// <summary>
    /// 把FileService.WebAPI当成一个云存储服务器，是一个Mock。文件保存在wwwroot文件夹下。
    /// 这仅供开发、演示阶段使用，在生产环境中，一定要用专门的云存储服务器来代替。
    /// </summary>
    public class MockCloudStorageClient : IStorageClient
    {
        private readonly IWebHostEnvironment _hostEnv;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MockCloudStorageClient(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostEnv)
        {
            _httpContextAccessor = httpContextAccessor;
            _hostEnv = hostEnv;
        }

        public StorageType StorageType => StorageType.Public;

        public async Task<Uri> SaveAsync(string key, Stream content, CancellationToken cancellationToken = default)
        {
            if (key.StartsWith('/'))
            {
                throw new ArgumentException("key shouldn't start with '/'", nameof(key));
            }
            string workingDir = Path.Combine(_hostEnv.ContentRootPath, "wwwroot");
            string fullPath = Path.Combine(workingDir, key);
            string? fullDir = Path.GetDirectoryName(fullPath);
            if (Directory.Exists(fullDir) == false)
            {
                Directory.CreateDirectory(fullDir);
            }
            if (File.Exists(fullPath))  // 如果文件已存在，则尝试删除
            {
                File.Delete(fullPath);
            }
            using Stream outStream = File.OpenWrite(fullPath);
            await content.CopyToAsync(outStream, cancellationToken);
            HttpRequest req = _httpContextAccessor.HttpContext.Request;
            string url = req.Scheme + "://" + req.Host + "/FileService/" + key;
            return new Uri(url);
        }
    }
}
