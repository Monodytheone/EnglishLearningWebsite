using IdentityService.Domain;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text;

namespace IdentityService.Infrastructure;

public class IdRepository : IIdRepository
{
    private readonly IdUserManager _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<IdRepository> _logger;
    private readonly IMemoryCache _memoryCache;

    public IdRepository(IdUserManager userManager, RoleManager<Role> roleManager, ILogger<IdRepository> logger, IMemoryCache memoryCache)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task<(IdentityResult, User?, string? password)> AddAdminUserAsync(string userName, string phoneNumber)
    {
        if (await this.FindByUserNameAsync(userName) != null)
        {
            return (ErrorIdentityResult($"用户名 {userName} 已存在"), null, null);
        }
        if (await this.FindByPhoneNumberAsync(phoneNumber) != null)
        {
            return (ErrorIdentityResult($"手机号 {phoneNumber} 已注册过"), null, null);
        }
        User user = new(userName);
        user.PhoneNumber = phoneNumber;
        user.PhoneNumberConfirmed = true;
        string password = this.GeneratePassword();
        IdentityResult createResult = await this.CreateUserAsync(user, password);
        if (createResult.Succeeded == false)
        {
            return (createResult, null, null);
        }
        IdentityResult addToAdminResult = await _userManager.AddToRoleAsync(user, "Admin");
        if (addToAdminResult.Succeeded == false)
        {
            return (addToAdminResult, null, null);
        }
        return (IdentityResult.Success, user, password);
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {
        if (await _roleManager.RoleExistsAsync(role) == false)
        {
            Role newRole = new Role{ Name = role };
            IdentityResult createRoleResult = await _roleManager.CreateAsync(newRole);
            if (createRoleResult.Succeeded == false)
            {
                return createRoleResult;
            }
        }
        return await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string newPassword)
    {
        if (newPassword.Length < 6)
        {
            IdentityError err = new();
            err.Code = "Password Inivalid";
            err.Description = "密码长度不能少于6";
            return IdentityResult.Failed(err);
        }
        string token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<SignInResult> ChangePhoneNumAsync(User user, string phoneNumber, string token)
    {
        IdentityResult changeResult = await _userManager.ChangePhoneNumberAsync(user, phoneNumber, token);
        if (changeResult.Succeeded == false)
        {
            _ = await _userManager.AccessFailedAsync(user);
            string errorMsg = changeResult.Errors.SumErrors();
            _logger.LogWarning($"{phoneNumber} ChangePhoneNumberAsync失败，错误信息: {errorMsg}");
            return SignInResult.Failed;
        }
        else
        {
            await this.ConfirmPhoneNumAsync(user.Id);
            return SignInResult.Success;
        }
    }


    public async Task<SignInResult> CheckForSignInAsync(User user, string password, bool lockoutOnFailure)
    {
        if (await _userManager.IsLockedOutAsync(user) == true)
        {
            return SignInResult.LockedOut;
        }

        if (await _userManager.CheckPasswordAsync(user, password) == true)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
            return SignInResult.Success;
        }
        else
        {
            if (lockoutOnFailure == true)
            {
                IdentityResult accessFailResult = await _userManager.AccessFailedAsync(user);
                if (accessFailResult.Succeeded == false)
                {
                    throw new ApplicationException("AccessFailed failed");
                }
            }
            return SignInResult.Failed;
        }
    }

    /// <summary>
    /// 确认手机号并更新到数据库
    /// </summary>
    public async Task ConfirmPhoneNumAsync(Guid id)
    {
        User? user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            throw new ArgumentException($"用户找不到， id={id}", nameof(id));
        }
        user.PhoneNumberConfirmed = true;
        await _userManager.UpdateAsync(user);   
    }

    public Task<IdentityResult> CreateUserAsync(User user, string password)
    {
        return _userManager.CreateAsync(user, password);
    }

    public Task<User?> FindByIdAsync(Guid userId)
    {
        return _userManager.FindByIdAsync(userId.ToString())!;
    }

    public Task<User?> FindByPhoneNumberAsync(string phoneNumber)
    {
        return _userManager.Users.FirstOrDefaultAsync(user => user.PhoneNumber == phoneNumber);
    }

    public Task<User?> FindByUserNameAsync(string userName)
    {
        return _userManager.FindByNameAsync(userName)!;
    }

    public Task<string> GenerateChangePhoneNumberTokenAsync(User user, string phoneNumber)
    {
        return _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
    }

    public Task<IList<string>> GetRoles(User user)
    {
        return _userManager.GetRolesAsync(user);
    }

    public string? RetrieveSmsCode(string phoneNumber)
    {
        string code = (string)_memoryCache.Get($"EngLearnSignInCode_{phoneNumber}");
        _memoryCache.Remove($"EngLearnSignInCode_{phoneNumber}");
        return code;
    }

    public void SaveSmsCode(string phoneNumber, string code)
    {
        _memoryCache.Set($"EngLearnSignInCode_{phoneNumber}", code, TimeSpan.FromMinutes(5));
    }

    private string GeneratePassword()
    {
        PasswordOptions options = _userManager.Options.Password;
        int length = options.RequiredLength;
        bool nonAlphanumeric = options.RequireNonAlphanumeric;
        bool digit = options.RequireDigit;
        bool lowercase = options.RequireLowercase;
        bool uppercase = options.RequireUppercase;

        StringBuilder password = new();
        Random random = new();
        while (password.Length < length)
        {
            char c = (char)random.Next(32, 126);
            password.Append(c);
            if (char.IsDigit(c))
                digit = false;
            else if (Char.IsLower(c))
                lowercase = false;
            else if (char.IsUpper(c))
                uppercase = false;
            else if (!char.IsLetterOrDigit(c))
                nonAlphanumeric = false;
        }

        if (nonAlphanumeric)
            password.Append((char)random.Next(33, 48));
        if (digit)
            password.Append((char)random.Next(48, 58));
        if (uppercase)
            password.Append((char)random.Next(65, 91));
        if (lowercase)
            password.Append((char)random.Next(97, 123));

        return password.ToString();        
    }

    private static IdentityResult ErrorIdentityResult(string msg)
    {
        var identityError = new IdentityError { Description = msg };
        return IdentityResult.Failed(identityError);
    }

    public async Task<IdentityResult> UserSoftDelete(Guid id)
    {
        User? user = await this.FindByIdAsync(id);
        IUserLoginStore<User> userLoginStore = _userManager.GetUserLoginStore();
        var noneCT = default(CancellationToken);

        //一定要删除aspnetuserlogins表中的数据，否则再次用这个外部登录登录的话
        //就会报错：The instance of entity type 'IdentityUserLogin<Guid>' cannot be tracked because another instance with the same key value for {'LoginProvider', 'ProviderKey'} is already being tracked.
        //而且要先删除aspnetuserlogins数据，再软删除User
        var logins = await userLoginStore.GetLoginsAsync(user, noneCT);
        foreach (var login in logins)
        {
            await userLoginStore.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey, noneCT);
        }
        user.SoftDelete();
        IdentityResult result = await _userManager.UpdateAsync(user);
        return result;
    }
}
