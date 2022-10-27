using Listening.Domain;
using Listening.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Listening.Infrastructure;

public class ListeningRepository : IListeningRepository
{
    private readonly ListeningDbContext _dbContext;

    public ListeningRepository(ListeningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Album?> GetAlbumByIdAsync(Guid albumId)
    {
        //return await _dbContext.Albums.SingleAsync(a => a.Id == albumId);
        return _dbContext.Albums.SingleOrDefaultAsync(a => a.Id == albumId);

    }

    public  Task<Album[]> GetAlbumsByCategoryIdAsync(Guid categoryId)
    {
        return _dbContext.Albums.Where(a => a.CategoryId == categoryId)
            .OrderBy(album => album.SequenceNumber).ToArrayAsync();
    }

    public async Task<Category[]> GetAllCategoriesAsync()
    {
        return await _dbContext.Categories.OrderBy(c => c.SequenceNumber).ToArrayAsync();
    }

    public Task<Category?> GetCategoryByIdAsync(Guid categoryId)
    {
        return _dbContext.Categories.SingleOrDefaultAsync(c => c.Id == categoryId);
    }

    public Task<Episode?> GetEpisodeByIdAsync(Guid episodeId)
    {
        return _dbContext.Episodes.SingleOrDefaultAsync(e => e.Id == episodeId);
    }

    public Task<Episode[]> GetEpisodesByAlbumIdAsync(Guid albumId)
    {
        return _dbContext.Episodes.Where(e => e.AlbumId == albumId)
            .OrderBy(episode => episode.SequenceNumber).ToArrayAsync();
    }

    public async Task<int> GetMaxSeqOfAlbumAsync(Guid categoryId)
    {
        int? maxSeq = await _dbContext.Albums.Where(album => album.CategoryId == categoryId)
            .MaxAsync(album => (int?)album.SequenceNumber);
        return maxSeq ?? 0;
    }

    public async Task<int> GetMaxSeqOfCategoryAsync()
    {
        //try
        //{
        //    return _dbContext.Category.MaxAsync(c => c.SequenceNumber);
        //}
        //catch (ArgumentNullException)
        //{
        //    return Task.FromResult(0);
        //}
        int? maxSeq = await _dbContext.Categories.MaxAsync(c => (int?)c.SequenceNumber);  // 就这么处理一条数据也没有的问题
        return maxSeq ?? 0;
    }

    public async Task<int> GetMaxSeqOfEpisodeAsync(Guid albumId)
    {
        int? maxSeq = await _dbContext.Episodes.Where(episode => episode.AlbumId == albumId)
            .MaxAsync(episode => (int?)episode.SequenceNumber);
        return maxSeq ?? 0;
    }
}
