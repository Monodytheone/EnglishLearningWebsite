using IdentityService.Domain;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure;

public class IdRepository : IIdRepository
{
    private readonly UserManager<User> _userManager;

    public IdRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
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

    public Task<User?> FindByPhoneNumberAsync(string phoneNumber)
    {
        return _userManager.Users.FirstOrDefaultAsync(user => user.PhoneNumber == phoneNumber);
    }

    public Task<User?> FindByUserNameAsync(string userName)
    {
        return _userManager.FindByNameAsync(userName);
    }

    public Task<IList<string>> GetRoles(User user)
    {
        return _userManager.GetRolesAsync(user);
    }
}
