using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SearchService.Domain;
using SearchService.WebAPI.Controllers.Requests;
using Zack.EventBus;

namespace SearchService.WebAPI.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class SearchController : ControllerBase
{
    private readonly ISearchRepository _repository;
    private readonly IEventBus _eventBus;

    // Validators of FluentValidation:
    private readonly IValidator<SearchEpisodesRequest> _searchEpisodesValidator;

    public SearchController(ISearchRepository repository, IValidator<SearchEpisodesRequest> searchEpisodesValidator, IEventBus eventBus)
    {
        _repository = repository;
        _searchEpisodesValidator = searchEpisodesValidator;
        _eventBus = eventBus;
    }

    [HttpGet]
    public async Task<ActionResult<SearchEpisodeInfosResponse>> SearchEpisodes([FromQuery] SearchEpisodesRequest request)  // request类也可以通过query传参并使用FluentValidation进行校验
    {
        var validationResult = await _searchEpisodesValidator.ValidateAsync(request);
        if (validationResult.IsValid == false)
        {
            return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
        }

        return Ok(_repository.SearchEpisodeInfosAsync(request.Keyword, request.PageIndex, request.PageSize));
    }

    [HttpPut]
    public async Task<ActionResult> ReIndexAll()
    {
        //避免耦合，这里发送ReIndexAll的集成事件
        //所有向搜索系统贡献数据的系统都可以响应这个事件，重新贡献数据
        _eventBus.Publish("SearchService.ReIndexAll", null);
        return Ok();
    }
}
