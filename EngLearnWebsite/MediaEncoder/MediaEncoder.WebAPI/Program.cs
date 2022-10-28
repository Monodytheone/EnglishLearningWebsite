using MediaEncoder.Infrastructure;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Zack.Commons;
using Zack.EventBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Zack.AnyDbConfigProvider--数据库配置源
builder.WebHost.ConfigureAppConfiguration((hostCtx, configBuilder) =>
{
    string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite")!;
    configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(2));
});

// Zack.EventBus--集成事件，RabbitMQ的封装
builder.Services.Configure<IntegrationEventRabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddEventBus("MediaEncoder", ReflectionHelper.GetAllReferencedAssemblies());

// 启用模块化的服务注册
builder.Services.RunModuleInitializers(ReflectionHelper.GetAllReferencedAssemblies());

// MediatR
builder.Services.AddMediatR(ReflectionHelper.GetAllReferencedAssemblies().ToArray());

// DbContext
builder.Services.AddDbContext<MEDbContext>(optionsBuilder =>
{
    string connStr = builder.Configuration.GetConnectionString("EngLearnWebsite");
    optionsBuilder.UseSqlServer(connStr);
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

// Zack.EventBus
app.UseEventBus();

app.Run();
