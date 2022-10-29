using FluentValidation;
using Listening.Admin.WebAPI;
using Listening.Admin.WebAPI.Controllers.Categories.Requests;
using Listening.Admin.WebAPI.Hubs;
using Listening.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Reflection;
using System.Text;
using Zack.ASPNETCore;
using Zack.Commons;
using Zack.EventBus;
using Zack.JWT;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 启用Zack.Commons提供的各项目自己进行自己提供的服务的注册
builder.Services.RunModuleInitializers(ReflectionHelper.GetAllReferencedAssemblies());

// MediatR
builder.Services.AddMediatR(ReflectionHelper.GetAllReferencedAssemblies());  // 这个AddMediatR是Zack.Infrastructure包里的扩展方法（其实就是ToArray了一下）

// Zack.AnyDbConfigProvider
builder.WebHost.ConfigureAppConfiguration((hostCtx, configBuilder) =>
{
    string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite")!;
    configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(2));
});

// JWT with SignalR身份认证
JWTOptions jwtOptions = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
//builder.Services.AddJWTAuthentication(jwtOptions);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(jwtBearerOpt =>
{
    JWTOptions jwtOptions = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
    byte[] keyBytes = Encoding.UTF8.GetBytes(jwtOptions.Key);
    var secKey = new SymmetricSecurityKey(keyBytes);
    jwtBearerOpt.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
        IssuerSigningKey = secKey,
    };

    // SignalR身份认证：
    jwtBearerOpt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // WebSocket不支持自定义报文头
            // 所以需要把jwt通过url的QueryString传递
            // 然后在服务端的OnMessageReceived中，把Query中的jwt读出来，赋给context.Token
            // 这样后续中间件才能从context.Token中解析出Token
            StringValues accessToken = context.Request.Query["access_token"];
            PathString path = context.HttpContext.Request.Path;
            if (string.IsNullOrEmpty(accessToken) == false
                && path.StartsWithSegments("/Hubs/EpisodeEncodingStatusHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

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

//Redis
string redisConnstr = builder.Configuration.GetValue<string>("Redis:ConnStr");
IConnectionMultiplexer redisConnMultipleser = ConnectionMultiplexer.Connect(redisConnstr);
builder.Services.AddSingleton(typeof(IConnectionMultiplexer), redisConnMultipleser);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

// 注册服务
builder.Services.AddScoped<EpisodeEncodeHelper>();

// Zack.EventBus
builder.Services.Configure<IntegrationEventRabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddEventBus("Listening.Admin", ReflectionHelper.GetAllReferencedAssemblies());

builder.Services.AddSignalR();


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

app.MapHub<EpisodeEncodingStatusHub>("/Hubs/EpisodeEncodingStatusHub");
app.MapControllers();

// Zack.EventBus
app.UseEventBus();

app.Run();
