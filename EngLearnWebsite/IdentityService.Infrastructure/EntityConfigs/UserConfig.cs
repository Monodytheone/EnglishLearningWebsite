using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure.EntityConfigs
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        // 看样子，这个配置类根本就没有起作用，有可能的话找找原因
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("T_Users");  // 不管用，算了
            builder.HasQueryFilter(user => user.IsDeleted == false);
        }
    }
}
