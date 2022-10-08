using IdentityService.Domain;
using IdentityService.Domain.Entities;
using IdentityService.WebAPI.Controllers.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace IdentityService.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IIdRepository _repository;
        private readonly IdDomainService _domainService;

        public LoginController(IIdRepository repository, IdDomainService domainService)
        {
            _repository = repository;
            _domainService = domainService;
        }

        /// <summary>
        /// 创建初始的管理员
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateWorld()
        {
            if (await _repository.FindByUserNameAsync("admin") != null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, "已经初始化过了");
            }
            User user = new("admin");
            IdentityResult result = await _repository.CreateUserAsync(user, "123456");
            Debug.Assert(result.Succeeded);
            string token = await _repository.GenerateChangePhoneNumberTokenAsync(user, "123456789");
            var changePhoneResult = await _repository.ChangePhoneNumAsync(user, "123456789", token);
            Debug.Assert(changePhoneResult.Succeeded);
            result = await _repository.AddToRoleAsync(user, "User");
            Debug.Assert(result.Succeeded);
            result = await _repository.AddToRoleAsync(user, "Admin");  // 所以说Role的名字大概是不区分大小写的
            Debug.Assert(result.Succeeded);
            return Ok("管理员初始化成功");
        }

        [HttpGet]
        [Authorize]  // 记得试一下软删除
        public async Task<ActionResult<UserInfoResponse>> GetUserInfo()
        {
            string id = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            User? user = await _repository.FindByIdAsync(Guid.Parse(id));
            if (user == null)  // 可能用户注销了
            {
                return NotFound("获取用户信息失败");
            }
            return new UserInfoResponse(Guid.Parse(id), user.PhoneNumber, user.CreationTime);
        }

        [HttpPost]
        public async Task<ActionResult<string>> LoginByPhoneNumAndPwd(LoginByPhoneNumAndPwdRequest request)
        {
            (SignInResult result, string? token)
                = await _domainService.LoginByPhoneAndPwd(request.PhoneNumber, request.Password);
            if (result.Succeeded)
            {
                return token!;
            }
            else if (result.IsLockedOut)
            {
                return StatusCode((int)HttpStatusCode.Locked, "账号已锁定，请稍后再试");
            }
            else
            {
                return BadRequest("登录失败");
            }
        }

        [HttpPost]
        public async Task<ActionResult<string>> LoginByUserNameAndPwd(LoginByUserNameAndPwdRequest request)
        {
            (SignInResult result, string? token)
                 = await _domainService.LoginByUserNameAndPwdAsync(request.UserName, request.Password);
            if (result.Succeeded)
            {
                return token!;
            }
            else if (result.IsLockedOut)
            {
                return StatusCode((int)HttpStatusCode.Locked, "账号已锁定，请稍后再试");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "登录失败");
            }
        }
    }
}
