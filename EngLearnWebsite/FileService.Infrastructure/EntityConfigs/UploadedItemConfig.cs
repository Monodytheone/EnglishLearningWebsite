using FileService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Infrastructure.EntityConfigs
{
    internal class UploadedItemConfig : IEntityTypeConfiguration<UploadedItem>
    {
        public void Configure(EntityTypeBuilder<UploadedItem> builder)
        {
            builder.ToTable("T_FS_UploadedItems");
            builder.HasKey(x => x.Id).IsClustered(false);  // 取消主键的聚集索引
            builder.Property(x => x.FileName).IsUnicode(true).HasMaxLength(1024);
            builder.Property(x => x.FileSHA256Hash).IsUnicode(false).HasMaxLength(64);
            builder.HasIndex(x => new { x.FileSHA256Hash, x.FileSizeInBytes });  // 经常要按照这两个列进行查询，因此把它们两个组成复合索引，提高查询效率。
        }
    }
}
