using FileService.SDK.NETCore;
using MediaEncoder.Domain;
using MediaEncoder.Domain.Entities;
using MediaEncoder.Infrastructure;
using MediaEncoder.WebAPI.Options;
using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using System.Net;
using Zack.Commons;
using Zack.EventBus;
using Zack.JWT;

namespace MediaEncoder.WebAPI.BackgroundServices;

/// <summary>
/// 进行转码的托管服务
/// <para>设计为串行转码，定时从数据库一条一条的取出EncodingItem，一条一条的转码。
/// 这么做是为了节省我们手里贫瘠的服务器资源</para>
/// </summary>
public class EncodingBackgroundService : BackgroundService
{
    private readonly IServiceScope _serviceScope;
    private readonly ILogger<EncodingBackgroundService> _logger;
    private readonly IMediaEncoderRepository _repository;
    private readonly MEDbContext _dbContext;
    private readonly List<RedLockMultiplexer> _redLockMultiplexerList;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEventBus _eventBus;
    private readonly MediaEncoderFactory _mediaEncoderFactory;
    private readonly IOptionsSnapshot<FileServiceOptions> _fileServiceOptions;
    private readonly IOptionsSnapshot<JWTOptions> _jwtOptions;
    private readonly ITokenService _tokenService;

    public EncodingBackgroundService(IServiceScopeFactory _scopeFactory)
    {
        _serviceScope = _scopeFactory.CreateScope();
        IServiceProvider serviceProvider = _serviceScope.ServiceProvider;

        _logger = serviceProvider.GetRequiredService<ILogger<EncodingBackgroundService>>();
        _repository = serviceProvider.GetRequiredService<IMediaEncoderRepository>();
        _dbContext = serviceProvider.GetRequiredService<MEDbContext>();
        _httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        _eventBus = serviceProvider.GetRequiredService<IEventBus>();
        _mediaEncoderFactory = serviceProvider.GetRequiredService<MediaEncoderFactory>();
        _fileServiceOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<FileServiceOptions>>();
        _jwtOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<JWTOptions>>();
        _tokenService = serviceProvider.GetRequiredService<ITokenService>();

        // RedLock:
        IConnectionMultiplexer connectionMultiplexer = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        _redLockMultiplexerList = new() { new RedLockMultiplexer(connectionMultiplexer) };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            _logger.LogInterpolatedInformation($"\n\n新一轮的扫描开始啦：");
            List<EncodingItem> readyItems = await _repository.FindByStatusAsync(EncodingStatus.Ready);
            foreach (EncodingItem readyItem in readyItems)
            {
                try
                {
                    await this.ProcessItemAsync(readyItem, stoppingToken);
                }
                catch (Exception ex)
                {
                    readyItem.Fail(ex);
                }
                await _dbContext.SaveChangesAsync(stoppingToken);
            }
            await Task.Delay(5000);  // 暂停5s，避免无任务时CPU空转
        }
    }

    private async Task ProcessItemAsync(EncodingItem encodingItem, CancellationToken ct)
    {
        Guid encodingItemId = encodingItem.Id;
        TimeSpan expiry = TimeSpan.FromSeconds(30);
        //Redis分布式锁来避免两个转码服务器处理同一个转码任务的问题
        // RedLock.net包
        RedLockFactory redLockFactory = RedLockFactory.Create(_redLockMultiplexerList);
        string lockKey = $"MediaEncoder.EncodingItem.{encodingItemId}";  // ps: 这个key与redis中存的其他所有东西都无关
        // 用RedLock分布式锁，锁定对EncodingItem的访问
        using var redLock = await redLockFactory.CreateLockAsync(lockKey, expiry);
        if (redLock.IsAcquired == false)
        {
            //_logger.LogInterpolatedWarning;
            _logger.LogWarning($"获取{lockKey}锁失败，已被抢走");
            //获得锁失败，锁已经被别人抢走了，说明这个任务被别的实例处理了（有可能有服务器集群来分担转码压力）
            return; // 再去抢下一个
        }

        encodingItem.Start();
        await _dbContext.SaveChangesAsync(ct);  // 立即保存一下状态的修改和发出领域事件

        // 下载源文件
        (bool downloadOk, FileInfo sourceFile) = await this.DownloadSourceAsync(encodingItem, ct);
        if (downloadOk == false)
        {
            encodingItem.Fail("下载失败");
            return;
        }

        FileInfo destFile = EncodingBackgroundService.BuildDestFileInfo(encodingItem);
        try
        {
            _logger.LogInterpolatedInformation($"下载encodingItemId= {encodingItemId} 成功，开始计算Hash值");
            long fileSizeInBytes = sourceFile.Length;
            string sourceFileHash = EncodingBackgroundService.ComputeSHA256Hash(sourceFile);
            EncodingItem? existedInstance = await _repository.FindCompletedItemAsync(sourceFileHash, fileSizeInBytes);
            if (existedInstance != null)
            {
                _logger.LogInterpolatedInformation($"检查EncodingItemId= {encodingItemId} Hash值成功，发现已经存在大小和Hash值相同的旧任务Id = {existedInstance.Id}，返回");  // 这两个Id本就应该相同的吧，哦，我想到了，万一某两个Episode对应的源文件是同一个呢
                _eventBus.Publish("MediaEncoding.Duplicated", new {Id = encodingItem.Id, SourceSystem = encodingItem.SourceSystem, OutputUrl = existedInstance.OutputUrl});
                return;
            }

            // 开始转码
            _logger.LogInterpolatedInformation($"EncodingItemId= {encodingItemId} 开始转码，源路径：{sourceFile}，目标路径：{destFile}");
            MediaFormat outputFormat = encodingItem.DestFormat;
            bool encodingOk = await this.EncodeAsync(sourceFile, destFile, outputFormat, ct);
            if (encodingOk == false)
            {
                encodingItem.Fail($"转码失败");
                return;
            }

            // 开始上传
            _logger.LogInterpolatedInformation($"EncodingItemId= {encodingItemId} 转码成功，开始准备上传");
            Uri destUrl = await this.UploadFileAsync(destFile, ct);
            encodingItem.Complete(destUrl);
            encodingItem.ChangeFileMeta(fileSizeInBytes, sourceFileHash);  // 记录的fileSize和Hash都是转码前文件的，因为要根据转码前的文件判断是否转码过相同的文件
            _logger.LogInterpolatedInformation($"EncodingItemid= {encodingItemId} 转码结果上传成功 ");
        }
        finally
        {
            sourceFile.Delete();
            destFile.Delete();
        }
    }

    /// <summary>
    /// 下载原文件
    /// </summary>
    /// <returns>ok表示是否下载成功， sourceFile为保存成功的本地文件</returns>
    private async Task<(bool ok, FileInfo sourceFile)> DownloadSourceAsync(EncodingItem encodingItem, CancellationToken ct)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "MediaEncodingDir");
        string sourceFullPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.{Path.GetExtension(encodingItem.Name)}");  // 源文件的临时保存路径
        FileInfo sourceFile = new(sourceFullPath);
        Guid encodingItemId = encodingItem.Id;
        sourceFile.Directory!.Create();  // 创建可能不存在的文件夹
        _logger.LogInterpolatedInformation($"EncodingItemId= {encodingItemId}，准备从{encodingItem.SourceUrl}下载到{sourceFullPath}");
        HttpClient httpClient = _httpClientFactory.CreateClient();
        HttpStatusCode statusCode = await httpClient.DownloadFileAsync(encodingItem.SourceUrl, sourceFullPath, ct);
        if (statusCode != HttpStatusCode.OK)
        {
            _logger.LogInterpolatedWarning($"下载EncodingItemId= {encodingItemId}，Url= {encodingItem.SourceUrl}失败，{statusCode}");
            sourceFile.Delete();
            return (false, sourceFile);
        }
        else
        {
            return (true, sourceFile);
        }

    }

    /// <summary>
    /// 构建转码后的目标文件名
    /// </summary>
    private static FileInfo BuildDestFileInfo(EncodingItem encodingItem)
    {
        string outputFormat = encodingItem.DestFormat.ToString();
        string tempDir = Path.GetTempPath();
        string destFullPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.{outputFormat}");
        return new FileInfo(destFullPath);
    }

    private static string ComputeSHA256Hash(FileInfo file)
    {
        using (FileStream streamSource = file.OpenRead())
        {
            return HashHelper.ComputeSha256Hash(streamSource);  
        }
    }

    private async Task<bool> EncodeAsync(FileInfo sourceFile, FileInfo destFile, MediaFormat outputFormat, CancellationToken ct)
    {
        IMediaEncoder? encoder = _mediaEncoderFactory.Create(outputFormat);
        if (encoder == null)
        {
            _logger.LogInterpolatedError($"转码失败，找不到转码器，目标格式：{outputFormat}");
            return false;
        }
        try
        {
            await encoder.EncodeAsync(sourceFile, destFile, outputFormat, null, ct);
        }
        catch (Exception exp)
        {
            _logger.LogError($"转码失败：{exp}");
            return false;
        }
        return true;
    }

    private Task<Uri> UploadFileAsync(FileInfo file, CancellationToken ct)
    {
        //Uri urlRoot = new(_fileServiceOptions.Value.UrlRoot);
        Uri urlRoot = new Uri("https://localhost:7114");



        //FileServiceClient fileService = new(urlRoot, _httpClientFactory, _tokenService, _jwtOptions.Value);

        // 既然你报空，那我直接给你一个，看你报不报
        JWTOptions mytempJWTOptions = new JWTOptions()
        {
            Issuer = "my",
            Audience = "my",
            ExpireSeconds = 31536000,
            Key = "sdfhihwijh32y78^(*&432y5hsdi79y09234y",
        };
        FileServiceClient fileService = new(urlRoot, _httpClientFactory, _tokenService, mytempJWTOptions);
        return fileService.UploadAsync(file, ct);
    }
    public override void Dispose()
    {
        _serviceScope.Dispose();
        base.Dispose();
    }
}
