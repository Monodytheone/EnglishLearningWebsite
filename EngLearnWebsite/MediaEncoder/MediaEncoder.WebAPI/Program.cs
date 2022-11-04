using MediaEncoder.Infrastructure;
using MediaEncoder.WebAPI.BackgroundServices;
using MediaEncoder.WebAPI.Options;
using MediatR;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Zack.Commons;
using Zack.EventBus;
using Zack.JWT;

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

// 配置项
builder.Services.Configure<FileServiceOptions>(builder.Configuration.GetSection("FileService:Endpoint"));
builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));  // 本项目中调用了FileService.SDK，需要传JWTOptions过去以让其构建token: 

// Zack.JWT: 本项目中调用了FileService.SDK，需要传TokenService过去以让其构建token
JWTOptions jwtOptions = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
builder.Services.AddJWTAuthentication(jwtOptions);


// 日志
builder.Services.AddLogging(logBuilder =>
{
    logBuilder.AddConsole();
});

// 托管服务调用FileService.SDK，要用到HttpClientFactory
builder.Services.AddHttpClient();

// Redis
string redisConnStr = builder.Configuration.GetValue<string>("Redis:ConnStr");
IConnectionMultiplexer redisConnMultiplexer = ConnectionMultiplexer.Connect(redisConnStr);
builder.Services.AddSingleton(typeof(IConnectionMultiplexer), redisConnMultiplexer);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

// 托管服务
builder.Services.AddHostedService<EncodingBackgroundService>();


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

// Zack.EventBus
app.UseEventBus();

app.Run();
