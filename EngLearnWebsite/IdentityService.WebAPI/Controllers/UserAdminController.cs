using IdentityService.Domain;
using IdentityService.Domain.Entities;
using IdentityService.Infrastructure;
using IdentityService.WebAPI.Controllers.Messages;
using IdentityService.WebAPI.MediatREvents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserAdminController : ControllerBase
{
    private readonly IdUserManager _userManager;
    private readonly IIdRepository _repository;
    private readonly IMediator _mediator;

    public UserAdminController(IdUserManager userManager, IIdRepository repository, IMediator mediator)
    {
        _userManager = userManager;
        _repository = repository;
        _mediator = mediator;
    }

    [HttpGet]
    public Task<UserDTO[]> FindAllUsers()
    {
        return _userManager.Users.Where(user => user.IsDeleted == false)
            .Select(user => UserDTO.Create(user)).ToArrayAsync();
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<UserDTO>> FindById(Guid id)
    {
        User? user = await _repository.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        else
        {
            return UserDTO.Create(user);
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateAdminUser(CreateAdminUserRequest request)
    {
        (IdentityResult result, User? user, string? password)
            = await _repository.AddAdminUserAsync(request.UserName, request.PhoneNumber);
        if (result.Succeeded == false)
        {
            return BadRequest(result.Errors.SumErrors());
        }
        await _mediator.Publish(new UserCreatedEvent(request.PhoneNumber, password!));
        return Ok();
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteUser([FromRoute]Guid id)
    {
        await _repository.UserSoftDelete(id);
        return Ok();
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<ActionResult> UpdateAdminUser(Guid id, EditAdminUserRequest request)
    {
        User? user = await _repository.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound("用户不存在");
        }
        user.PhoneNumber = request.PhoneNumber;
        IdentityResult result = await _userManager.UpdateAsync(user);
        if (result.Succeeded == false)
        {
            return BadRequest(result.Errors.SumErrors());
        }
        else return Ok();
    }

    [HttpPost]
    [Route("{id}")]
    public async Task<ActionResult> ResetAdminUserPassword(Guid id)
    {
        var outCome = await _repository.ResetPasswordAsync(id);
        if (outCome.idResult.Succeeded == false)
        {
            return BadRequest(outCome.idResult.Errors.SumErrors());
        }
        await _mediator.Publish(new PasswordResetEvent(outCome.user!.PhoneNumber, outCome.user.UserName, outCome.password!));
        return Ok();
    }
}
