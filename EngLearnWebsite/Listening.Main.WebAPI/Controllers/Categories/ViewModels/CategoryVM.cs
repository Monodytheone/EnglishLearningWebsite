using Listening.Domain.Entities;
using Listening.Infrastructure.Migrations;
using Zack.DomainCommons.Models;

namespace Listening.Main.WebAPI.Controllers.Categories.ViewModels;

public record CategoryVM(Guid Id, MultilingualString Name, Uri CoverUrl)
{
    /// <summary>
    /// 根据Category创建起ViewModel
    /// </summary>
    public static CategoryVM? Create(Category? category)
    {
        if (category == null)
        {
            return null;
        }
        return new CategoryVM(category.Id, category.Name, category.CoverUrl);
    }

    public static CategoryVM[] Create(Category[] categories)
    {
        return categories.Select(c => CategoryVM.Create(c)!).ToArray();
    }
}
