using FluentValidation;
using Listening.Admin.WebAPI.Controllers.Albums.Requests;
using Listening.Domain;
using Listening.Domain.Entities;
using Listening.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Zack.ASPNETCore;

namespace Listening.Admin.WebAPI.Controllers.Albums
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [UnitOfWork(typeof(ListeningDbContext))]
    public class AlbumController : ControllerBase
    {
        private readonly ListeningDomainService _domainService;
        private readonly IListeningRepository _repository;
        private readonly ListeningDbContext _dbContext;

        // FluentValidator手动模式的校验器
        private readonly IValidator<AlbumAddRequest> _addValidator;
        private readonly IValidator<AlbumUpdateRequest> _updateValidator;
        private readonly IValidator<AlbumSortRequest> _sortValidator;

        public AlbumController(ListeningDomainService domainService, IListeningRepository repository, ListeningDbContext dbContext, IValidator<AlbumAddRequest> addValidator, IValidator<AlbumUpdateRequest> updateValidator, IValidator<AlbumSortRequest> sortValidator)
        {
            _domainService = domainService;
            _repository = repository;
            _dbContext = dbContext;
            _addValidator = addValidator;
            _updateValidator = updateValidator;
            _sortValidator = sortValidator;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Album>> FindById([RequiredGuid] Guid id)
        {
            Album? album = await _repository.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound("Id不存在");
            }
            return Ok(album);
        }

        [HttpGet]
        [Route("{categoryId}")]
        public async Task<ActionResult<Album[]>> FindByCategoryId([RequiredGuid] Guid categoryId)
        {
            if (await _repository.GetCategoryByIdAsync(categoryId) == null)
            {
                return NotFound("CategoryId不存在");
            }
            return await _repository.GetAlbumsByCategoryIdAsync(categoryId);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Add(AlbumAddRequest request)
        {
            var validationResult = await _addValidator.ValidateAsync(request);
            if (validationResult.IsValid == false)
            {
                return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
            }

            Album album = await _domainService.AddAlbumAsync(request.CategoryId, request.Name);
            _dbContext.Albums.Add(album);
            return album.Id;
        }

        [HttpPut]
        [Route("{id}")]  // 试图RESTful一点
        public async Task<ActionResult> Update([RequiredGuid] Guid id, AlbumUpdateRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (validationResult.IsValid == false)
            {
                return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
            }

            Album? album = await _repository.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound("id不存在");
            }

            album.ChangeName(request.Name);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> DeleteById([RequiredGuid] Guid id)
        {
            Album? album = await _repository.GetAlbumByIdAsync(id);
            if (album == null)
            {
                //return Ok();
                return NotFound("id不存在");  // 这样做仍然是幂等的，因为“调用N次，确保服务器处于与第一次调用相同的状态。”与响应无关
            }

            album.SoftDelete();
            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Hide([RequiredGuid] Guid id)
        {
            Album? album = await _repository.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound("未找到对应的album");
            }

            _ = album.Hide();
            return Ok();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Show([RequiredGuid] Guid id)
        {
            Album? album = await _repository.GetAlbumByIdAsync(id);
            if (album == null)
            {
                return NotFound("未找到album");
            }

            _ = album.Show();
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Sort(AlbumSortRequest request)
        {
            var validationResult = await _sortValidator.ValidateAsync(request);
            if (validationResult.IsValid == false)
            {
                return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
            }

            await _domainService.SortAlbumsAsync(request.CagetoryId, request.SortedAlbumIds);
            return Ok();
        }
    }
}
