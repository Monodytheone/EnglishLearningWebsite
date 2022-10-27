using Listening.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listening.Infrastructure.EntityConfigs
{
    internal class EpisodeConfig : IEntityTypeConfiguration<Episode>
    {
        public void Configure(EntityTypeBuilder<Episode> builder)
        {
            builder.ToTable("T_Episodes");
            builder.HasKey(e => e.Id).IsClustered(false);  // 取消Guid主键的聚集索引
            builder.HasIndex(e => new { e.AlbumId, e.IsDeleted });  // 创建索引

            builder.OwnsOne(e => e.Name, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.Property(name => name.Chinese)
                    .IsRequired().HasMaxLength(200).IsUnicode();
                ownedNavigationBuilder.Property(name => name.English)
                    .IsRequired().HasMaxLength(200).IsUnicode(false);
            });
            builder.Navigation(e => e.Name).IsRequired();

            builder.Property(e => e.AudioUrl).IsRequired().HasMaxLength(1000).IsUnicode();
            builder.OwnsOne(e => e.Subtitle, navigationBuilder =>
            {
                navigationBuilder.Property(subtitle => subtitle.Format)
                    .IsRequired().HasConversion<string>().IsUnicode(false)  // 枚举转为字符串存储
                    .HasMaxLength(10);
                navigationBuilder.Property(subtitle => subtitle.Content)
                    .IsRequired().IsUnicode(true).HasMaxLength(int.MaxValue);
            });
        }
    }
}
