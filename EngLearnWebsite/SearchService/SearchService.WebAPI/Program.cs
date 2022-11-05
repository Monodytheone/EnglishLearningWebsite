using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using SearchService.Infrastructure;
using SearchService.WebAPI.Controllers.Requests;
using Zack.Commons;
using Zack.EventBus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 数据库配置Provider
builder.WebHost.ConfigureAppConfiguration((hostCtx, configBuilder) =>
{
    string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite")!;
    configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(2));
});

// 模块化服务注册
builder.Services.RunModuleInitializers(ReflectionHelper.GetAllReferencedAssemblies());

// 配置项绑定
builder.Services.Configure<ElasticSearchOptions>(builder.Configuration.GetSection("ElasticSearch"));

// Zack.EventBus--集成事件
builder.Services.Configure<IntegrationEventRabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddEventBus("SearchService.WebAPI", ReflectionHelper.GetAllReferencedAssemblies());

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<SearchEpisodesRequest>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseEventBus();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
