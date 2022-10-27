using Listening.Domain.Entities;

namespace Listening.Domain;

public interface IListeningRepository
{
    Task<int> GetMaxSeqOfAlbumAsync(Guid categoryId);
    Task<int> GetMaxSeqOfCategoryAsync();
    Task<int> GetMaxSeqOfEpisodeAsync(Guid albumId);


    Task<Album[]> GetAlbumsByCategoryIdAsync(Guid categoryId);

    /// <summary>
    /// 获得所有的Category
    /// </summary>
    /// <returns>根据SequenceNumber排好序的Category数组</returns>
    Task<Category[]> GetAllCategoriesAsync();
    Task<Episode[]> GetEpisodesByAlbumIdAsync(Guid albumId);


    Task<Album?> GetAlbumByIdAsync(Guid albumId);
    Task<Category?> GetCategoryByIdAsync(Guid categoryId);  
    Task<Episode?> GetEpisodeByIdAsync(Guid episodeId);



}
