using Listening.Domain.Entities;
using Zack.DomainCommons.Models;

namespace Listening.Main.WebAPI.Controllers.Albums.ViewModels;

public record AlbumVM(Guid Id, MultilingualString Name, Guid CategoryId)
{
    public static AlbumVM? Create(Album? album)
    {
        if (album == null)
        {
            return null;
        }
        else
            return new AlbumVM(album.Id, album.Name, album.CategoryId);
    }

    public static List<AlbumVM> Create(List<Album> albums)
    {
        return albums.Select(album => AlbumVM.Create(album)!).ToList();
    }
}