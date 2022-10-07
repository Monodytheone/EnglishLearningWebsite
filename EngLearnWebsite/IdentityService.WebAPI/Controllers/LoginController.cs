using IdentityService.Domain;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;

namespace IdentityService.WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IIdRepository _repository;

        public LoginController(IIdRepository repository)
        {
            _repository = repository;
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
            IdentityResult createUserResult = await _repository.CreateUserAsync(user, "123456");
            Debug.Assert(createUserResult.Succeeded);
            string token = await _repository.GenerateChangePhoneNumberTokenAsync(user, "123456789");
            var changePhoneResult = await _repository.ChangePhoneNumAsync(user, "123456789", token);
            Debug.Assert(changePhoneResult.Succeeded);

        }
    }
}
