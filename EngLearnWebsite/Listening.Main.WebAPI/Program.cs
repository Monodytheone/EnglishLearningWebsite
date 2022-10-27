using Listening.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zack.ASPNETCore;
using Zack.Commons;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 启用Zack.Commons提供的各项目自己进行自己提供的服务的注册
builder.Services.RunModuleInitializers(ReflectionHelper.GetAllReferencedAssemblies());

builder.Services.AddMediatR(ReflectionHelper.GetAllReferencedAssemblies());

builder.Services.AddDbContext<ListeningDbContext>(optionsBuilder =>
{
    string connStr = builder.Configuration.GetConnectionString("EngLearnWebsite");
    optionsBuilder.UseSqlServer(connStr);
});

// 注册Zack.AspNetCore中的内存缓存帮助类
builder.Services.AddScoped<IMemoryCacheHelper, MemoryCacheHelper>();  

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

app.UseAuthorization();

app.MapControllers();

app.Run();
