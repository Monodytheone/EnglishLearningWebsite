using Listening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Listening.Infrastructure.EntityConfigs
{
    internal class AlbumConfig : IEntityTypeConfiguration<Album>
    {
        public void Configure(EntityTypeBuilder<Album> builder)
        {
            builder.ToTable("T_Albums");
            builder.HasKey(x => x.Id).IsClustered(false);  // 不建Guid主键的聚集索引
            builder.OwnsOne(album => album.Name, navigationBuilder =>
            {
                navigationBuilder.Property(name => name.Chinese).HasMaxLength(200).IsRequired().IsUnicode();
                navigationBuilder.Property(name => name.English).HasMaxLength(200).IsRequired().IsUnicode(false);
            }).Navigation(album => album.Name).IsRequired();
            builder.HasIndex(album => new { album.CategoryId, album.IsDeleted });  // 建立索引
        }
    }
}
