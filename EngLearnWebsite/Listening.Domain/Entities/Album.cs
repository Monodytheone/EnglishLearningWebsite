using Zack.DomainCommons.Models;

namespace Listening.Domain.Entities
{
    /// <summary>
    /// 专辑（二级目录）
    /// </summary>
    public record Album : AggregateRootEntity, IAggregateRoot
    {
        private Album() { }

        /// <summary>
        /// 用户是否可见
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// 标题
        /// </summary>
        public MultilingualString Name { get; private set; }

        /// <summary>
        /// 列表中的显示序号
        /// </summary>
        public int SequenceNumber { get; private set; }

        /// <summary>
        /// 所属类别的Id（不直接持有其他聚合根的引用，而只是持有其Id）
        /// </summary>
        public Guid CategoryId { get; private set; }

        public static Album Create(Guid id, MultilingualString name, int sequenceNumber, Guid CategoryId)
        {
            Album album = new();  // 私有构造
            album.Id = id;
            album.IsVisible = false;  // 专辑新建后默认不可见，需要手动Show
            album.Name = name;
            album.SequenceNumber = sequenceNumber;
            album.CategoryId = CategoryId;
            return album;
        }

        public Album ChangeSequenceNumber(int value)
        {
            this.SequenceNumber = value;
            return this;
        }

        public Album ChangeName(MultilingualString value)
        {
            this.Name = value;
            return this;
        }

        public Album Show()
        {
            this.IsVisible = true;
            return this;
        }

        public Album Hide()
        {
            this.IsVisible = false;
            return this;
        }
    }
}
