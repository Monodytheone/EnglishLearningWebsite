using IdentityService.Domain;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure;

public class IdRepository : IIdRepository
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<IdRepository> _logger;

    public IdRepository(UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<IdRepository> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
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
        return _userManager.FindByIdAsync(userId.ToString());
    }

    public Task<User?> FindByPhoneNumberAsync(string phoneNumber)
    {
        return _userManager.Users.FirstOrDefaultAsync(user => user.PhoneNumber == phoneNumber);
    }

    public Task<User?> FindByUserNameAsync(string userName)
    {
        return _userManager.FindByNameAsync(userName);
    }

    public Task<string> GenerateChangePhoneNumberTokenAsync(User user, string phoneNumber)
    {
        return _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
    }

    public Task<IList<string>> GetRoles(User user)
    {
        return _userManager.GetRolesAsync(user);
    }
}
