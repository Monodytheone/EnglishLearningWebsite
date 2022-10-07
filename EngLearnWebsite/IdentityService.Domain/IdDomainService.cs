using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Zack.JWT;

namespace IdentityService.Domain
{
    public class IdDomainService
    {
        private readonly IIdRepository _repository;
        private readonly ITokenService _tokenService;  // 这玩意儿需不需要自己注册一下？
        private readonly IOptionsSnapshot<JWTOptions> _jwtOptions;

        public IdDomainService(IIdRepository idRepository, ITokenService tokenService, IOptionsSnapshot<JWTOptions> jwtOptions)
        {
            _repository = idRepository;
            _tokenService = tokenService;
            _jwtOptions = jwtOptions;
        }

        public async Task<(SignInResult, string?)> LoginByPhoneAndPwd(string phoneNumber, string password)
        {
            User? user = await _repository.FindByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                return (SignInResult.Failed, null);
            }

            SignInResult signInResult = await _repository.CheckForSignInAsync(user, password, lockoutOnFailure: true);
            if (signInResult.Succeeded)
            {
                string jwt = await this.BuildTokenAsync(user);
                return (SignInResult.Success, jwt);
            }
            else
            {
                return (signInResult, null);
            }
        }

        public async Task<(SignInResult, string?)> LoginByUserNameAndPwdAsync(string userName, string password)
        {
            User? user = await _repository.FindByUserNameAsync(userName);
            if (user == null)
            {
                return (SignInResult.Failed, null);
            }

            SignInResult signInResult = await _repository.CheckForSignInAsync(user, password, lockoutOnFailure: true);
            if (signInResult.Succeeded)
            {
                string token = await this.BuildTokenAsync(user);
                return (SignInResult.Success, token);
            }
            else
            {
                return (signInResult, null);
            }
        }

        private async Task<string> BuildTokenAsync(User user)
        {
            IList<string> roles = await _repository.GetRoles(user);
            List<Claim> claims = new();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            string token = _tokenService.BuildToken(claims, _jwtOptions.Value);
            return token;
        }
    }
}
