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
        private readonly ISmsSender _smsSender;

        public IdDomainService(IIdRepository idRepository, ITokenService tokenService, IOptionsSnapshot<JWTOptions> jwtOptions, ISmsSender smsSender)
        {
            _repository = idRepository;
            _tokenService = tokenService;
            _jwtOptions = jwtOptions;
            _smsSender = smsSender;
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

        public async Task<SignInResult> SendSignInSmsCodeAsync(string phoneNumber)  // 记得用这个试试软删除
        {
            User? user = await _repository.FindByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                return SignInResult.Failed;
            }
            string code = Random.Shared.Next(1, 9999).ToString();
            _repository.SaveSmsCode(phoneNumber, code);
            await _smsSender.SendAsync(phoneNumber, code);
            return SignInResult.Success;
        }

        public async Task<(SignInResult, string?)> LoginByPhoneNumAndSmsCodeAsync(string phoneNumber, string code)
        {
            User? user = await _repository.FindByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                return (SignInResult.Failed, null);
            }
            string? serverSideCode = _repository.RetrieveSmsCode(phoneNumber);
            if (serverSideCode == null)
            {
                return (SignInResult.Failed, null);
            }
            if (code == serverSideCode)
            {
                string token = await this.BuildTokenAsync(user);
                return (SignInResult.Success, token);
            }
            else
            {
                return (SignInResult.Failed, null);
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
