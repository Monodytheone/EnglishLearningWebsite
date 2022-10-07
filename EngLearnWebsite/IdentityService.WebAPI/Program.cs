using IdentityService.Domain.Entities;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
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
    builder.Host.UseNLog();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.RunModuleInitializers(ReflectionHelper.GetAllReferencedAssemblies());  // ����ģ�黯ע��

    // ����������������ʶ������á���������������
    builder.Services.AddDbContext<IdDbContext>(optionsBuilder =>
    {
        string connStr = builder.Configuration.GetConnectionString("EngLearnWebsite");
        optionsBuilder.UseSqlServer(connStr);
    });  // �ҷ�Ҫ�Ѵ����ݿ������ж�����д�����ݿ����õ�����֮ǰ�в���? ���еģ���һ���������ô���ˣ�˵��builder.Build()֮ǰ��˳������ν��
    builder.Services.AddDataProtection();  // ��������ڱ����Ŀ����������
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
        .AddDefaultTokenProviders()  // ��������ڱ����Ŀ����������
        .AddUserManager<UserManager<User>>()
        .AddRoleManager<RoleManager<Role>>();
    // ������������������������������������������������������������

    builder.WebHost.ConfigureAppConfiguration((hostCtx, configBuilder) =>
    {
        string connStr = Environment.GetEnvironmentVariable("ConnectionStrings:EngLearnWebsite");
        configBuilder.AddDbConfiguration(() => new SqlConnection(connStr), reloadOnChange: true, reloadInterval: TimeSpan.FromSeconds(2));
    });  // ����Zack.AnyDbConfigProvider

    builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));  //��JWT������



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
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "��ΪNLog��Program.cs��Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}
