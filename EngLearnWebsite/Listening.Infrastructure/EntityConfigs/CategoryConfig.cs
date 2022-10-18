using Listening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listening.Infrastructure.EntityConfigs;

internal class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("T_Categories");
        builder.HasKey(c => c.Id).IsClustered(false);  // 取消Guid主键的聚集索引（此方法为SqlServer独有）
        builder.OwnsOne(c => c.Name, navigationBuilder =>
        {
            navigationBuilder.Property(name => name.Chinese)
                .IsUnicode(true)
                .IsRequired()
                .HasMaxLength(200);
            navigationBuilder.Property(name => name.English)
                .IsUnicode(false)
                .IsRequired()
                .HasMaxLength(200);
        }).Navigation(c => c.Name).IsRequired();
        builder.Property(c => c.CoverUrl).IsRequired(false).HasMaxLength(500).IsUnicode(true);
    }
}
