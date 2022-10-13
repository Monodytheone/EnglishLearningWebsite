using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Domain
{
    /// <summary>
    /// 仓储接口
    /// </summary>
    public interface IIdRepository
    {
        Task<User?> FindByIdAsync(Guid userId);

        /// <summary>
        /// 根据手机号寻找用户
        /// </summary>
        Task<User?> FindByPhoneNumberAsync(string phoneNumber);

        /// <summary>
        /// 根据用户名寻找用户
        /// </summary>
        Task<User?> FindByUserNameAsync(string userName);

        /// <summary>
        /// 为了登录，检查用户名和密码
        /// </summary>
        /// <param name="lockoutOnFailure">true时，登录失败则记录一次失败（达到一定次数则锁定）</param>
        Task<SignInResult> CheckForSignInAsync(User user, string password, bool lockoutOnFailure);

        /// <summary>
        /// 获取User的Role
        /// </summary>
        Task<IList<string>> GetRoles(User user);

        /// <summary>
        /// 创建用户
        /// </summary>
        Task<IdentityResult> CreateUserAsync(User user, string password);

        Task<string> GenerateChangePhoneNumberTokenAsync(User user, string phoneNumber);

        Task<SignInResult> ChangePhoneNumAsync(User user, string phoneNumber, string token);

        /// <summary>
        /// 确认手机号
        /// </summary>
        Task ConfirmPhoneNumAsync(Guid id);

        Task<IdentityResult> AddToRoleAsync(User user, string role);

        Task<IdentityResult> ChangePasswordAsync(User user, string newPassword);

        void SaveSmsCode(string phoneNumber, string code);

        string? RetrieveSmsCode(string phoneNumber);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="phoneNumber"></param>
        /// <returns>返回的第三个值是生成的密码</returns>
        Task<(IdentityResult, User?, string? password)> AddAdminUserAsync(string userName, string phoneNumber);

        Task<IdentityResult> UserSoftDelete(Guid id);

        Task<(IdentityResult idResult, User? user, string? password)> ResetPasswordAsync(Guid id);
    }
}
