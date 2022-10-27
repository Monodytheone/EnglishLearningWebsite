using Listening.Domain.Entities.ValueObjects;
using Listening.Domain.Notifications;
using System.Xml.Linq;
using Zack.DomainCommons.Models;

namespace Listening.Domain.Entities;

/// <summary>
/// 音频
/// </summary>
public record Episode : AggregateRootEntity, IAggregateRoot
{
    private Episode() { }

    /// <summary>
    /// 显示序号
    /// </summary>
    public int SequenceNumber { get; private set; }

    /// <summary>
    /// 标题
    /// </summary>
    public MultilingualString Name { get; private set; }

    /// <summary>
    /// 所属专辑的Id
    /// </summary>
    public Guid AlbumId { get; private set; }

    /// <summary>
    /// 音频路径
    /// </summary>
    public Uri AudioUrl { get; private set; }

    /// <summary>
    /// 音频时长（秒）
    /// </summary>
    public double DurationInSecond { get; private set; }

    ///// <summary>
    ///// 转码之前的字幕原文
    ///// </summary>
    //public string Subtitle { get; private set; }

    ///// <summary>
    ///// 字幕格式
    ///// </summary>
    //public SubtitleType SubtitleType { get; private set; } 

    /// <summary>
    /// 把字幕原文和字幕格式做成值对象了
    /// </summary>
    public Subtitle Subtitle { get; private set; }

    /// <summary>
    /// 用户是否可见（默认可见--写在Builder.Build里）
    /// </summary>
    public bool IsVisible { get; private set; }

    public Episode ChangeSequenceNumber(int value)
    {
        this.SequenceNumber = value;
        this.AddDomainEventIfAbsent(new EpisodeUpdateNotification(this));
        return this;
    }

    public Episode ChangeName(MultilingualString value)
    {
        this.Name = value;
        this.AddDomainEventIfAbsent(new EpisodeUpdateNotification(this));
        return this;
    }

    public Episode ChangeSubtitle(Subtitle value)
    {
        // 这里不做格式支持的判断了，因为Format是个枚举。判断放在转枚举的时候吧
        this.Subtitle = value;
        this.AddDomainEventIfAbsent(new EpisodeUpdateNotification(this));
        return this;
    }

    public Episode Hide()
    {
        this.IsVisible = false;
        this.AddDomainEventIfAbsent(new EpisodeUpdateNotification(this));
        return this;
    }

    public Episode Show()
    {
        this.IsVisible = true;
        this.AddDomainEventIfAbsent(new EpisodeUpdateNotification(this));
        return this;
    }

    public override void SoftDelete()
    {
        base.SoftDelete();
        this.SequenceNumber = -1;  // 软删除的序号都设为-1，这样数据库看起来不乱
        this.AddDomainEventIfAbsent(new EpisodeSoftDeleteNotification(this.Id));
    }

    public class Builder
    {
        private Guid _id;
        private int _sequenceNumber;
        private MultilingualString _name;
        private Guid _albumId;
        private Uri _audioUrl;
        private double _durationInSecond;
        private Subtitle _subtitle;

        public Builder Id(Guid value)
        {
            _id = value;
            return this;
        }
        public Builder SequenceNumber(int value)
        {
            _sequenceNumber = value;
            return this;
        }
        public Builder Name(MultilingualString value)
        {
            _name = value;
            return this;
        }
        public Builder AlbumId(Guid id)
        {
            _albumId = id;
            return this;
        }
        public Builder AudioUrl(Uri value)
        {
            _audioUrl = value;
            return this;
        }
        public Builder DurationInSecond(double value)
        {
            _durationInSecond = value;
            return this;
        }
        public Builder Subtitle(Subtitle value)
        {
            _subtitle = value;
            return this;
        }
        
        public Episode Build()
        {
            if (_id == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(_id));
            }
            if (_name == null)
            {
                throw new ArgumentNullException(nameof(_name));
            }
            if (_albumId == Guid.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(_albumId));
            }
            if (_audioUrl == null)
            {
                throw new ArgumentNullException(nameof(_audioUrl));
            }
            if (_durationInSecond <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_durationInSecond));
            }
            if (_subtitle == null)
            {
                throw new ArgumentNullException(nameof(_subtitle));
            }
            Episode e = new();
            e.Id = _id;
            e.SequenceNumber = _sequenceNumber;
            e.Name = _name;
            e.AlbumId = _albumId;
            e.AudioUrl = _audioUrl;
            e.DurationInSecond = _durationInSecond;
            e.Subtitle = _subtitle;
            e.IsVisible = true;
            e.AddDomainEvent(new EpisodeCreateNotification(e));
            return e;
        }
    }
}
