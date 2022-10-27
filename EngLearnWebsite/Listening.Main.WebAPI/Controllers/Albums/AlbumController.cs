using Listening.Domain;
using Listening.Domain.Entities;
using Listening.Main.WebAPI.Controllers.Albums.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Zack.ASPNETCore;

namespace Listening.Main.WebAPI.Controllers.Albums
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly IListeningRepository _repository;
        private readonly IMemoryCacheHelper _memoryCacheHelper;

        public AlbumController(IListeningRepository repository, IMemoryCacheHelper memoryCacheHelper)
        {
            _repository = repository;
            _memoryCacheHelper = memoryCacheHelper;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<AlbumVM>> FindById([RequiredGuid] Guid id)
        {
            //AlbumVM? albumVM = await _memoryCacheHelper.GetOrCreateAsync($"AlbumController.FindById.{id}",
            //    async cacheEntry => AlbumVM.Create(await _repository.GetAlbumByIdAsync(id)));
            AlbumVM? albumVM = await _memoryCacheHelper.GetOrCreateAsync($"AlbumController.FindById.{id}",
                async cacheEntry =>
                {
                    Album? album = await _repository.GetAlbumByIdAsync(id);
                    if (album == null || album.IsVisible == false)
                    {
                        return null;
                    }
                    return AlbumVM.Create(album);
                });
            if (albumVM == null)
            {
                return NotFound("Album不存在或不可见");
            }
            return Ok(albumVM);
        }

        [HttpGet]
        [Route("{categoryId}")]
        public async Task<ActionResult<List<AlbumVM>>> FindByCategoryId([RequiredGuid] Guid categoryId)
        {
            List<AlbumVM>? albumVMs = await _memoryCacheHelper.GetOrCreateAsync($"AlbumController.FindByCategoryId.{categoryId}",
                async cacheEntry =>
                {
                    Album[] albums = await _repository.GetAlbumsByCategoryIdAsync(categoryId);
                    
                    // 凡是IsVisible == false的Album，移出队列：
                    List<Album> albumList = albums.ToList();
                    List<Album> removeList = new();
                    foreach (Album album in albumList)
                    {
                        if (album.IsVisible == false)
                        {
                            removeList.Add(album);
                        }
                    }
                    foreach (Album albumToRemove in removeList)
                    {
                        albumList.Remove(albumToRemove);
                    }
                    return AlbumVM.Create(albumList);
                });
            if (albumVMs.Count == 0)
            {
                return NotFound("未找到该category下的album");
            }
            return albumVMs!;
        }
    }
}
