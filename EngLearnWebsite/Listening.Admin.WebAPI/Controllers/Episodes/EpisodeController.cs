using FluentValidation;
using Listening.Admin.WebAPI.Controllers.Episodes.Requests;
using Listening.Domain;
using Listening.Domain.Entities;
using Listening.Domain.Entities.ValueObjects;
using Listening.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Zack.ASPNETCore;
using Zack.EventBus;

namespace Listening.Admin.WebAPI.Controllers.Episodes
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [UnitOfWork(typeof(ListeningDbContext))]
    public class EpisodeController : ControllerBase
    {
        private readonly ListeningDomainService _domainService;
        private readonly IListeningRepository _repository;
        private readonly ListeningDbContext _dbContext;
        private readonly EpisodeEncodeHelper _episodeEncodeHelper;
        private readonly IEventBus _eventBus;

        // Validators of FluentValidation:
        private readonly IValidator<EpisodeAddRequest> _addValidator;
        private readonly IValidator<EpisodeUpdateRequest> _updateValidator;
        private readonly IValidator<EpisodeSortRequest> _sortValidator;

        public EpisodeController(ListeningDomainService domainService, IListeningRepository repository, ListeningDbContext dbContext, IValidator<EpisodeAddRequest> addValidator, EpisodeEncodeHelper episodeEncodeHelper, IEventBus eventBus, IValidator<EpisodeUpdateRequest> updateValidator, IValidator<EpisodeSortRequest> sortValidator)
        {
            _domainService = domainService;
            _repository = repository;
            _dbContext = dbContext;
            _addValidator = addValidator;
            _episodeEncodeHelper = episodeEncodeHelper;
            _eventBus = eventBus;
            _updateValidator = updateValidator;
            _sortValidator = sortValidator;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Add(EpisodeAddRequest request)
        {
            var validationResult = await _addValidator.ValidateAsync(request);
            if (validationResult.IsValid == false)
            {
                return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
            }

            // 如果上传的是m4a，不用转码，直接存到数据库
            if (request.AudioUrl.ToString().EndsWith("m4a", StringComparison.OrdinalIgnoreCase))
            {
                Episode episode = await _domainService.AddEpisodeAsync(request.Name, request.AlbumId, request.AudioUrl, request.DurationInSecond, new Domain.Entities.ValueObjects.Subtitle(request.Subtitle_Content, request.Subtitle_Format));
                _dbContext.Episodes.Add(episode);
                return episode.Id;
            }
            else
            {
                Guid episodeId = Guid.NewGuid();
                Subtitle subtitle = new(request.Subtitle_Content, request.Subtitle_Format);
                EncodingEpisodeInfo encodingEpisode = new(episodeId, request.Name, request.AlbumId, request.DurationInSecond, subtitle, EncodeStatus.Created);
                await _episodeEncodeHelper.AddEncodingEpisodeAsync(encodingEpisode);  // 把转码信息存到Redis

                // 发布集成事件，通知转码
                _eventBus.Publish("MediaEncoding.Created", new { MediaId = episodeId, MediaUrl = request.AudioUrl, OutPutFormat = "m4a", SourceSystem = "Listening" });

                return episodeId;
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update([RequiredGuid] Guid id, EpisodeUpdateRequest request)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (validationResult.IsValid == false)
            {
                return BadRequest(validationResult.Errors.Select(error => error.ErrorMessage));
            }

            Episode? episode = await _repository.GetEpisodeByIdAsync(id);
            if (episode == null)
            {
                return NotFound("EpisodeId不存在");
            }

            episode.ChangeName(request.Name).ChangeSubtitle(new Subtitle(request.Subtitle_Content, request.Subtitle_Format));
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult> Delete([RequiredGuid] Guid id)
        {
            Episode? episode = await _repository.GetEpisodeByIdAsync(id);
            if (episode == null)
            {
                // 这样做仍然是幂等的，因为“调用N次，确保服务器处于与第一次调用相同的状态。”与响应无关
                return NotFound("EpisodeId不存在");
            }

            episode.SoftDelete();
            return Ok();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<Episode>> FindById([RequiredGuid] Guid id)
        {
            Episode? episode = await _repository.GetEpisodeByIdAsync(id);
            if (episode == null)
            {
                return NotFound("EpisodeId不存在");
            }
            return episode;  // 这是后台系统，不在乎把实体类整个返回给客户端的问题
        }

        [HttpGet]
        [Route("{albumId}")]
        public async Task<ActionResult<Episode[]>> FindByAlbumId([RequiredGuid][FromRoute(Name = "albumId")] Guid albumId)
        {
            if (await _dbContext.Albums.FindAsync(albumId) == null)
            {
                return NotFound("albumId不存在");
            }
            Episode[] episodes = await _repository.GetEpisodesByAlbumIdAsync(albumId);
            return episodes;
        }

        /// <summary>
        /// 获取Album下的所有转码任务
        /// </summary>
        /// <param name="albumId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{albumId}")]
        public async Task<ActionResult<EncodingEpisodeInfo[]>> FindEncodingEpisodesByAlbumId([RequiredGuid] Guid albumId)
        {
            if (await _repository.GetAlbumByIdAsync(albumId) == null)
            {
                return NotFound("albumId不存在");
            }

            List<EncodingEpisodeInfo> resultList = new();
            IEnumerable<Guid> episodesIds = await _episodeEncodeHelper.GetEncodingEpisodeIdsByAlbumIdAsync(albumId);
            foreach (Guid episodeId in episodesIds)
            {
                EncodingEpisodeInfo encodingEpisode =
                    await _episodeEncodeHelper.GetEncodingEpisodeAsync(episodeId);
                if (encodingEpisode.Status != EncodeStatus.Completed)  // 不显示已完成转码的
                {
                    resultList.Add(encodingEpisode);    
                }
            }
            return resultList.ToArray();
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Hide([RequiredGuid] Guid id)
        {
            Episode? episode = await _repository.GetEpisodeByIdAsync(id);
            if (episode == null)
            {
                return NotFound("episodeId不存在");
            }

            episode.Hide();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Show([RequiredGuid] Guid id)
        {
            Episode? episode = await _repository.GetEpisodeByIdAsync(id);
            if (episode == null)
            {
                return NotFound("episodeId不存在");
            }

            episode.Show();
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Sort(EpisodeSortRequest request)
        {
            var validatorResult = await _sortValidator.ValidateAsync(request);
            if (validatorResult.IsValid == false)
            {
                return BadRequest(validatorResult.Errors.Select(error => error.ErrorMessage));
            }

            await _domainService.SortEpisodesAsync(request.AlbumId, request.SortedEpisodeIds);
            return Ok();
        }   
    }
}
