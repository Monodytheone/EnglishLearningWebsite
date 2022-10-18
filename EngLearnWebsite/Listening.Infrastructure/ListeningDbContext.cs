using Listening.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zack.Infrastructure.EFCore;

namespace Listening.Infrastructure;

public class ListeningDbContext : BaseDbContext
{
    public DbSet<Category> Categories { get; private set; }
    public DbSet<Album> Albums { get; private set; }
    public DbSet<Episode> Episodes { get; private set; }

    public ListeningDbContext(DbContextOptions<ListeningDbContext> options, IMediator? mediator) : base(options, mediator)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        modelBuilder.EnableSoftDeletionGlobalFilter();  // 这是Zack.Infrastructure包里的扩展方法，为所有实现了ISoftDelete的实体类都添加软删除的全局查询筛选器。
        // 没错，终于发现了，软删除的全局查询筛选器是这么实现的
    }
}
