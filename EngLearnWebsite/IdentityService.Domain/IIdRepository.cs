﻿using IdentityService.Domain.Entities;
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
    }
}
