using FileService.Domain;
using FileService.Infrastructure;
using FileService.Infrastructure.Services;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using System.Reflection;
using Zack.Commons;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<FSDbContext>(optionsBuilder =>
{
    string connStr = builder.Configuration.GetConnectionString("EngLearnWebsite");
    optionsBuilder.UseSqlServer(connStr);
});
builder.Host.ConfigureAppConfiguration((hostCtx, configBuilder) =>
{
    //不能使用ConfigureAppConfiguration中的configBuilder去读取配置，否则就循环调用了，因此这里直接自己去读取配置文件
    //var configRoot = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    //string connStr = configRoot.GetValue<string>("DefaultDB:ConnStr");
    string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite");
    //string connStr = "Server=.;Database=EngLearnWebsite;Trusted_Connection=True;MultipleActiveResultSets=true;Integrated Security=SSPI";
    configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(5));
});
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.RunModuleInitializers(ReflectionHelper.GetAllReferencedAssemblies());
builder.Services.Configure<SMBStorageOptions>(builder.Configuration.GetSection("FileService:SMB"));
builder.Services.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblies(ReflectionHelper.GetAllReferencedAssemblies());
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
