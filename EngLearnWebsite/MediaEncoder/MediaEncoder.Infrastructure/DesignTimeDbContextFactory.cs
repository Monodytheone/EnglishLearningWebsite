using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MediaEncoder.Infrastructure
{
    internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MEDbContext>
    {
        public MEDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<MEDbContext>();
            string ConnStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite")!;
            builder.UseSqlServer(ConnStr);
            return new MEDbContext(builder.Options, null);
        }
    }
}
