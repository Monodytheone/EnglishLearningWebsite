using AsmResolver.PE.Relocations;
using FluentValidation;
using Listening.Admin.WebAPI.Controllers.Categories.Requests;
using Listening.Domain;
using Listening.Domain.Entities;
using Listening.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Zack.ASPNETCore;

namespace Listening.Admin.WebAPI.Controllers.Categories;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles="Admin")]
[UnitOfWork(typeof(ListeningDbContext))]
public class CategoryController : ControllerBase
{
    private readonly IListeningRepository _repository;
    private readonly ListeningDomainService _domainService;
    private readonly IValidator<CategoryAddRequest> _addRequestValidator;
    private readonly IValidator<CategoryUpdateRequest> _updateRequestValidator;
    private readonly ListeningDbContext _dbContext;

    public CategoryController(IListeningRepository repository, ListeningDomainService domainService, IValidator<CategoryAddRequest> addRequestValidator, ListeningDbContext dbContext, IValidator<CategoryUpdateRequest> updateRequestValidator)
    {
        _repository = repository;
        _domainService = domainService;
        _addRequestValidator = addRequestValidator;
        _dbContext = dbContext;
        _updateRequestValidator = updateRequestValidator;
    }

    [HttpGet]
    public Task<Category[]> FindAll()
    {
        return _repository.GetAllCategoriesAsync();
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<Category>> FindById([RequiredGuid]Guid id)
    {
        Category? category = await _repository.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound($"没有Id={id}的Category");
        }
        return Ok(category);    
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Add(CategoryAddRequest request)
    {
        // FluentValidation的手动校验模式
        var validationResult = await _addRequestValidator.ValidateAsync(request);
        if (validationResult.IsValid == false)
        {
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
        }

        Category category = await _domainService.AddCategoryAsync(request.Name, request.CoverUrl);
        _dbContext.Add(category);  // 普通的增删改查而已，没必要非要写进仓储里，这就是洋葱架构的灵活性
        return Ok(category.Id);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<ActionResult> Update([RequiredGuid] Guid id, CategoryUpdateRequest request)
    {
        // FluentValidation的手动校验模式
        var validationResult = await _updateRequestValidator.ValidateAsync(request);
        if (validationResult.IsValid == false)
        {
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
        }

        Category? category = await _repository.GetCategoryByIdAsync(id);
        if (category == null)
        {
            return NotFound($"categoryId={id}不存在");
        }
        category.ChangeName(request.Name).ChangeCoverUrl(request.CoverUrl).NotifyModified();
        return Ok();
    }
}
