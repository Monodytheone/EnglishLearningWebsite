using Listening.Domain;
using Listening.Domain.Entities;
using Listening.Main.WebAPI.Controllers.Categories.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Zack.ASPNETCore;

namespace Listening.Main.WebAPI.Controllers.Categories
{
    // 尽管这里的FindAll、FindById和Admin.WebAPI中类似，但是不应该复用。因为Admin.WebAPI中的操作没有缓存，
    // 而Main.WebAPI中的则由缓存
    [Route("api/[controller]/[action]")]
    [ApiController]
    // 用户界面只有查没有改，就不用写UnitOfWork了吧
    public class CategoryController : ControllerBase
    {
        private readonly IListeningRepository _repository;
        private readonly IMemoryCacheHelper _memoryCacheHelper;

        public CategoryController(IMemoryCacheHelper memoryCacheHelper, IListeningRepository repository)
        {
            _memoryCacheHelper = memoryCacheHelper;
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<CategoryVM[]>> FindAll()
        {
            Task<Category[]> FindData()
            {
                return _repository.GetAllCategoriesAsync();
            }
            var task = _memoryCacheHelper.GetOrCreateAsync("CategoryController.FindAll",
                async cacheEntry => CategoryVM.Create(await FindData()));
            return await task;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<CategoryVM>> FindById([RequiredGuid] Guid id)
        {
            CategoryVM? categoryVM = await _memoryCacheHelper.GetOrCreateAsync($"CategoryController.FindById.{id}",
                  async (cacheEntry) => CategoryVM.Create(await _repository.GetCategoryByIdAsync(id)));
            if (categoryVM == null)
            {
                return NotFound("Category不存在");
            }
            return Ok(categoryVM);  
        }
    }
}
