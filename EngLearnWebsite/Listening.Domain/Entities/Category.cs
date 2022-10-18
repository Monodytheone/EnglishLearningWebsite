
using Zack.DomainCommons.Models;

namespace Listening.Domain.Entities;

/// <summary>
/// 类别（最高级目录）
/// </summary>
public record Category : AggregateRootEntity, IAggregateRoot
{
    private Category() { }

    /// <summary>
    /// 在所有Category中的显示序号，越小越靠前
    /// </summary>
    public int SequenceNumber { get; private set; }

    public MultilingualString Name { get; private set; }

    /// <summary>
    /// 封面图片的路径
    /// </summary>
    public Uri CoverUrl { get; private set; }

    public static Category Create(Guid id, int sequenceNumber, MultilingualString name, Uri coverUrl)
    {
        Category category = new();  // 这里调的是私有构造方法
        category.Id = id;
        category.Name = name;
        category.SequenceNumber = sequenceNumber;
        category.CoverUrl = coverUrl;
        return category;
    }

    public Category ChangeSequenceNumber(int newValue)
    {
        this.SequenceNumber = newValue;
        return this;
    }

    public Category ChangeName(MultilingualString newName)
    {
        this.Name = newName;
        return this;
    }

    public Category ChangeCoverUrl(Uri newUrl)
    {
        this.CoverUrl = newUrl;
        return this;
    }
}
