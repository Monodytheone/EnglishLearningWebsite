using Listening.Domain.Entities;
using Listening.Domain.Entities.ValueObjects;
using Zack.DomainCommons.Models;

namespace Listening.Domain;

public class ListeningDomainService
{
    private readonly IListeningRepository _repository;

    public ListeningDomainService(IListeningRepository repository)
    {
        _repository = repository;
    }

    public async Task<Album> AddAlbumAsync(Guid categoryId, MultilingualString name)
    {
        Album newAlbum = Album.Create(Guid.NewGuid(), name, await _repository.GetMaxSeqOfAlbumAsync(categoryId) + 1, categoryId);
        return newAlbum;
    }

    /// <summary>
    ///  按照第二个参数数组里的顺序重新排序
    /// </summary>
    public async Task SortAlbumsAsync(Guid categoryId, Guid[] sortedAlbumsIds)
    {
        Album[] albumsInDb = await _repository.GetAlbumsByCategoryIdAsync(categoryId);
        IEnumerable<Guid> idsInDb = albumsInDb.Select(album => album.Id);
        if (idsInDb.SequenceIgnoreEqual(sortedAlbumsIds) == false)
        {
            throw new Exception($"提交的待排序Id必须是 categoryId={categoryId}分类下所有的Id");
        }

        int seqNum = 1;
        // 一个in语句一次性取出来更快，不过在非性能关键节点，业务语言比性能更重要
        foreach (Guid albumId in sortedAlbumsIds)
        {
            Album? album = await _repository.GetAlbumByIdAsync(albumId);
            if (album == null)
            {
                throw new Exception($"albumId={albumId} 不存在");
            }
            album.ChangeSequenceNumber(seqNum);
            seqNum++;
        }
    }

    public async Task<Category> AddCategoryAsync(MultilingualString name, Uri coverUrl)
    {
        int sequenceNumber = await _repository.GetMaxSeqOfCategoryAsync() + 1;
        return Category.Create(Guid.NewGuid(), sequenceNumber, name, coverUrl);
    }

    public async Task SortCategoriesAsync(Guid[] sortedCategoriesIds)
    {
        IEnumerable<Category> categoriesInDb = await _repository.GetAllCategoriesAsync();
        IEnumerable<Guid> categoryIdsInDb = categoriesInDb.Select(c => c.Id);
        if (categoryIdsInDb.SequenceIgnoreEqual(sortedCategoriesIds) == false)
        {
            throw new Exception("提交的待排序Id中必须是所有的分类Id");
        }

        int seqNum = 1;
        foreach (Guid categoryId in sortedCategoriesIds)
        {
            Category? category = await _repository.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                throw new Exception($"categoryId={categoryId} 不存在");
            }
            category.ChangeSequenceNumber(seqNum);
            seqNum++;
        }
    }

    public async Task<Episode> AddEpisodeAsync(MultilingualString name, Guid albumId, Uri audioUrl, double durationInSeconde, Subtitle subtitle)
    {
        int seqNum = await _repository.GetMaxSeqOfEpisodeAsync(albumId) + 1;
        Episode episode = new Episode.Builder()
            .Id(Guid.NewGuid()).SequenceNumber(seqNum).Name(name).AlbumId(albumId).AudioUrl(audioUrl)
            .DurationInSecond(durationInSeconde).Subtitle(subtitle)
            .Build();
        return episode;
    }

    public async Task SortEpisodesAsync(Guid albumId, Guid[] sortedEpisodesIds)
    {
        IEnumerable<Episode> episodesInDb = await _repository.GetEpisodesByAlbumIdAsync(albumId);
        IEnumerable<Guid> idsInDb = episodesInDb.Select(e => e.Id);
        if (idsInDb.SequenceIgnoreEqual(sortedEpisodesIds) == false)
        {
            throw new Exception("提交的待排序Id必须是该Album中所有Episode的Id");
        }
        int seqNum = 1;
        foreach (Guid id in sortedEpisodesIds)
        {
            Episode? episode = await _repository.GetEpisodeByIdAsync(id);
            if (episode == null)
            {
                throw new Exception($"episodeId={id} 不存在");
            }
            episode.ChangeSequenceNumber(seqNum);
            seqNum++;
        }
    }
}
