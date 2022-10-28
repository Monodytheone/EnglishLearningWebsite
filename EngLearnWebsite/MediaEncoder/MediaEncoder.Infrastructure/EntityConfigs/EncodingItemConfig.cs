using MediaEncoder.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaEncoder.Infrastructure.EntityConfigs;

internal class EncodingItemConfig : IEntityTypeConfiguration<EncodingItem>
{
    public void Configure(EntityTypeBuilder<EncodingItem> builder)
    {
        builder.ToTable("T_ME_EncodingItems");
        builder.HasKey(x => x.Id).IsClustered(false);  // Guid主键设为非聚集索引
        //builder.HasIndex(x => new { x.Status, x.FileSHA256Hash, x.FileSizeInByte });  // 复合索引
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.FileSHA256Hash, x.FileSizeInByte });
        builder.Property(x => x.Name).HasMaxLength(256);
        builder.Property(x => x.FileSHA256Hash).HasMaxLength(64).IsUnicode(false);
        builder.Property(x => x.DestFormat)
            .HasConversion<string>().HasMaxLength(10).IsUnicode(false);
        builder.Property(x => x.Status)
            .HasConversion<string>().HasMaxLength(20).IsUnicode(false);
        builder.Property(x => x.OutputUrl).HasMaxLength(1000).IsUnicode();
        builder.Property(x => x.SourceUrl).HasMaxLength(1000).IsUnicode();
        //builder.Property(x => x.LogText).IsUnicode();  // 看看默认的是什么（应该会是nvarchar吧）
    }
}
