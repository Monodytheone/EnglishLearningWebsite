using MediaEncoder.Domain.Notifications;
using Zack.DomainCommons.Models;

namespace MediaEncoder.Domain.Entities;

/// <summary>
/// Id与对应的Episode相同
/// </summary>
public record EncodingItem : BaseEntity, IAggregateRoot, IHasCreationTime
{
    public DateTime CreationTime { get; private set; }

    public string SourceSystem { get; private set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long? FileSizeInByte { get; private set; }

    /// <summary>
    /// 文件名（不是全路径）
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 文件的SHA256散列值
    /// 
    /// <para> 两个文件的大小和散列值（SHA256）都相同的概率非常小。因此只要大小和SHA256相同，就认为是相同的文件。
    /// SHA256的碰撞的概率比MD5低很多。</para>
    /// </summary>
    public string? FileSHA256Hash { get; private set; }

    /// <summary>
    /// 待转码的文件
    /// </summary>
    public Uri SourceUrl { get; private set; }

    /// <summary>
    /// 转码完成的文件路径
    /// </summary>
    public Uri? OutputUrl { get; private set; }

    /// <summary>
    /// 转码的目标类型
    /// </summary>
    public MediaFormat DestFormat { get; private set; }

    public EncodingStatus Status { get; private set; }

    /// <summary>
    /// 转码器的输出日志
    /// </summary>
    public string? LogText { get; private set; }


    public void Start()
    {
        this.Status = EncodingStatus.Started;
        this.AddDomainEvent(new EncodingItemStartNotification(this.Id, this.SourceSystem));  // 为什么不用IfAbsent呢
    }

    public void Complete(Uri outputUrl)
    {
        this.Status = EncodingStatus.Completed;
        this.OutputUrl = outputUrl;
        this.LogText = "转码成功";
        this.AddDomainEvent(new EncodingItemCompleteNotification(this.Id, this.SourceSystem, outputUrl));
    }

    public void Fail(string logText)
    {
        this.Status = EncodingStatus.Failed;
        this.LogText = logText;
        this.AddDomainEventIfAbsent(new EncodingItemFailNotification(this.Id, this.SourceSystem, logText));
    }

    public void Fail(Exception ex)
    {
        this.Fail($"转码失败：{ex}");
    }

    public void ChangeFileMeta(long fileSizeInByte, string sha256Hash)
    {
        this.FileSizeInByte = fileSizeInByte;
        this.FileSHA256Hash = sha256Hash;
    }

    public static EncodingItem Create(Guid id, string name, Uri sourceUrl, MediaFormat destFormat, string sourceSystem)
    {
        EncodingItem item = new()
        {
            Id = id,
            CreationTime = DateTime.Now,
            Name = name,
            SourceUrl = sourceUrl,
            DestFormat = destFormat,
            SourceSystem = sourceSystem,
            Status = EncodingStatus.Ready,
        };
        item.AddDomainEvent(new EncodingItemCreateNotification(item));
        return item;
    }
}
