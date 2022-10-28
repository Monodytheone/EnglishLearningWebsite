using MediaEncoder.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Zack.Commons;
using Zack.Infrastructure.EFCore;

namespace MediaEncoder.Infrastructure
{
    public class MEDbContext : BaseDbContext
    {
        public DbSet<EncodingItem> EncodingItems { get; private set; }

        public MEDbContext(DbContextOptions<MEDbContext> options, IMediator? mediator) : base(options, mediator)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
            // 这个服务没有使用ISoftDelete
        }
    }
}
