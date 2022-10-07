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
    }
}
