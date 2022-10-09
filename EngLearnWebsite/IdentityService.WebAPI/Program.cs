using FluentValidation;
using FluentValidation.AspNetCore;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure;
using IdentityService.WebAPI.Controllers.Messages;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using NLog.Web.LayoutRenderers;
using System.Reflection;
using Zack.Commons;
using Zack.JWT;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");
try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();  // 这么一搞控制台的输出直接没了，变到NLog里了

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.RunModuleInitializers(ReflectionHelper.GetAllReferencedAssemblies());  // 启用模块化注册

    // ↓↓↓↓↓↓↓标识框架配置↓↓↓↓↓↓↓↓
    builder.Services.AddDbContext<IdDbContext>(optionsBuilder =>
    {
        string connStr = builder.Configuration.GetConnectionString("EngLearnWebsite");
        optionsBuilder.UseSqlServer(connStr);
    });  // 我非要把从数据库配置中读配置写到数据库配置的配置之前行不行? 答：行的，上一个服务就这么干了，说明builder.Build()之前的顺序无所谓的
    builder.Services.AddDataProtection();  // 这个东西在别的项目里点儿不出来
    builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;

        options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    });
    IdentityBuilder idBuilder = new(typeof(User), typeof(Role), builder.Services);
    idBuilder.AddEntityFrameworkStores<IdDbContext>()
        .AddDefaultTokenProviders()  // 这个东西在别的项目里点儿不出来
        .AddUserManager<IdUserManager>()
        .AddRoleManager<RoleManager<Role>>();
    // ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

    builder.WebHost.ConfigureAppConfiguration((hostCtx, configBuilder) =>
    {
        string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite")!;
        configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(2));
    });  // 配置Zack.AnyDbConfigProvider

    builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));  //绑定JWT配置项

    // ↓↓↓↓↓↓↓↓ FluentValidation ↓↓↓↓↓↓↓↓↓↓
    builder.Services.AddFluentValidationAutoValidation();
    //builder.Services.AddScoped<IValidator<LoginByPhoneNumAndPwdRequest>, LoginByPhoneNumAndPwdRequestValidator>();  // 注册一个Validator
    builder.Services.AddValidatorsFromAssemblyContaining<LoginByPhoneNumAndPwdRequestValidator>();  // 从包含LoginByPhoneNumAndPwdRequestValidator的程序集中加载所有Validator
    // ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

    JWTOptions jwtOptions = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
    builder.Services.AddJWTAuthentication(jwtOptions);  // Zack.JWT中对JWT的配置

    builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

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

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "因为NLog的Program.cs，Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}
