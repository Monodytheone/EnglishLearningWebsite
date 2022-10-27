using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listening.Infrastructure;

internal class DesignTimDbContextFactory : IDesignTimeDbContextFactory<ListeningDbContext>
{
    public ListeningDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ListeningDbContext>();
        string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite")!;
        builder.UseSqlServer(connStr);
        return new ListeningDbContext(builder.Options, null);
    }
}
