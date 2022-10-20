using FluentValidation;
using Listening.Admin.WebAPI.Controllers.Categories.Requests;
using Listening.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Zack.ASPNETCore;
using Zack.Commons;
using Zack.JWT;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 启用Zack.Commons提供的各项目自己进行自己提供的服务的注册
builder.Services.RunModuleInitializers(ReflectionHelper.GetAllReferencedAssemblies());

builder.Services.AddMediatR(ReflectionHelper.GetAllReferencedAssemblies());

// Zack.AnyDbConfigProvider
builder.WebHost.ConfigureAppConfiguration((hostCtx, configBuilder) =>
{
    string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite")!;
    configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(2));
});

// JWT
JWTOptions jwtOptions = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
builder.Services.AddJWTAuthentication(jwtOptions);

builder.Services.AddDbContext<ListeningDbContext>(optionsBuilder =>
{
    string connStr = builder.Configuration.GetConnectionString("EngLearnWebsite");
    optionsBuilder.UseSqlServer(connStr);
});

// 本项目尝试使用FluentValidation的手动校验方式。因为自动校验已不推荐使用
builder.Services.AddValidatorsFromAssemblyContaining<CategoryAddRequest>();

// Filter
builder.Services.Configure<MvcOptions>(mvcOptions =>
{
    mvcOptions.Filters.Add<UnitOfWorkFilter>();  // 启用工作单元Filter
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
