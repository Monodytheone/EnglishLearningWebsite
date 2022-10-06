using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Infrastructure
{
    internal class FSDesignTimeDbContextFactory : IDesignTimeDbContextFactory<FSDbContext>
    {
        public FSDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<FSDbContext>();
            string connStr = Environment.GetEnvironmentVariable("ConnetionStrings:EngLearnWebsite");
            builder.UseSqlServer(connStr);
            return new FSDbContext(builder.Options, null);
        }
    }
}
