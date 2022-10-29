using FileService.Domain;
using FileService.Infrastructure;
using FileService.Infrastructure.Services;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Zack.ASPNETCore;
using Zack.Commons;
using Zack.JWT;

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

// Zack.AnyDbCongigProvider
builder.Host.ConfigureAppConfiguration((hostCtx, configBuilder) =>
{
    string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite")!;
    configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(5));
});

// MediatR
builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

// 模块化服务注册
builder.Services.RunModuleInitializers(ReflectionHelper.GetAllReferencedAssemblies());

builder.Services.Configure<SMBStorageOptions>(builder.Configuration.GetSection("FileService:SMB"));

// FluentValidation的自动模式
builder.Services.AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblies(ReflectionHelper.GetAllReferencedAssemblies());
});

// Filter
builder.Services.Configure<MvcOptions>(mvcOptions =>
{
    mvcOptions.Filters.Add<UnitOfWorkFilter>();
});

// Zack.JWT
JWTOptions jwtOptions = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
builder.Services.AddJWTAuthentication(jwtOptions);


// 让Swagger中带上Authorization报文头
builder.Services.AddSwaggerGen(opt =>
{
    OpenApiSecurityScheme scheme = new()
    {
        Description = "Authorization报文头. \r\n例如：Bearer ey234927349dhhsdid",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Authorization" },
        Scheme = "oauth2",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
    };
    opt.AddSecurityDefinition("Authorization", scheme);
    OpenApiSecurityRequirement requirement = new();
    requirement[scheme] = new List<string>();
    opt.AddSecurityRequirement(requirement);
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
