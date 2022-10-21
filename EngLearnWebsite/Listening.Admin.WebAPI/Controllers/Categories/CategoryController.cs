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
    private readonly IValidator<CategoriesSortRequest> _sortRequestValidator;
    private readonly ListeningDbContext _dbContext;

    public CategoryController(IListeningRepository repository, ListeningDomainService domainService, IValidator<CategoryAddRequest> addRequestValidator, ListeningDbContext dbContext, IValidator<CategoryUpdateRequest> updateRequestValidator, IValidator<CategoriesSortRequest> sortRequestValidator)
    {
        _repository = repository;
        _domainService = domainService;
        _addRequestValidator = addRequestValidator;
        _dbContext = dbContext;
        _updateRequestValidator = updateRequestValidator;
        _sortRequestValidator = sortRequestValidator;
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

    /// <summary>
    /// 幂等的软删除
    /// </summary>
    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteById([RequiredGuid] Guid id)
    {
        Category? category = await _repository.GetCategoryByIdAsync(id);
        if (category == null)
        {//这样做仍然是幂等的，因为“调用N次，确保服务器处于与第一次调用相同的状态。”与响应无关
            return NotFound($"id不存在");
        }
        category.SoftDelete();
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> Sort(CategoriesSortRequest request)
    {
        var validationResult = await _sortRequestValidator.ValidateAsync(request);   
        if (validationResult.IsValid == false)
        {
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
        }

        await _domainService.SortCategoriesAsync(request.SortedCategoryIds);
        // 目前，_domainService.SortCategoriesAsync如果执行失败是会直接把抛的错500返回给前端的。若要想处理，则需要让这个方法返回结果，在Action里根据结果来返回值

        return Ok();
    }
}
