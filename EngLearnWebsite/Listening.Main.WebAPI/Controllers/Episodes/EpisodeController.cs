using Listening.Domain;
using Listening.Domain.Entities;
using Listening.Main.WebAPI.Controllers.Albums.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Zack.ASPNETCore;

namespace Listening.Main.WebAPI.Controllers.Episodes
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EpisodeController : ControllerBase
    {
        private readonly IMemoryCacheHelper _memoryCacheHelper;
        private readonly IListeningRepository _repository;

        public EpisodeController(IMemoryCacheHelper memoryCacheHelper, IListeningRepository repository)
        {
            _memoryCacheHelper = memoryCacheHelper;
            _repository = repository;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<EpisodeVM>> FindById([RequiredGuid] Guid id)
        {
            EpisodeVM? vm = await _memoryCacheHelper.GetOrCreateAsync($"EpisodeController.FindById.{id}", async cacheEntry =>
            {
                Episode? episode = await _repository.GetEpisodeByIdAsync(id);
                if (episode == null)
                {
                    return null;
                }
                return EpisodeVM.Create(episode, true);
            });

            if (vm == null)
            {
                return NotFound("episode不存在或不可见");
            }
            return vm;
        }

        [HttpGet]
        [Route("{albumId}")]
        public async Task<ActionResult<List<EpisodeVM>>> FindByAlbumId([RequiredGuid] Guid albumId)
        {
            //_repository.GetAlbumByIdAsync(albumId)
            //_repository.GetEpisodesByAlbumIdAsync(albumId)
            List<EpisodeVM>? vms = await _memoryCacheHelper.GetOrCreateAsync($"EpisodeController.FindByAlbumId.{albumId}", async cacheEntry =>
            {
                Album? album = await _repository.GetAlbumByIdAsync(albumId);
                if (album == null)
                {
                    return null;
                }
                else if(album.IsVisible == false)
                {
                    return null;
                }

                Episode[] episodes = await _repository.GetEpisodesByAlbumIdAsync(albumId);
                return EpisodeVM.Create(episodes.ToList(), false);//加载Episode列表的，默认不加载Subtitle，这样降低流量大小
            });

            if (vms == null)
            {
                return NotFound("album不存在或不可见");
            }
            else if (vms.Count == 0)
            {
                return NotFound("album中没有可见的episode");
            }
            else
                return vms;
        }
    }
}
