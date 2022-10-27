using Listening.Domain.Entities;
using Zack.DomainCommons.Models;

namespace Listening.Main.WebAPI.Controllers.Episodes.ViewModels;

public record EpisodeVM(Guid Id, MultilingualString Name, Guid AlbumId, Uri AudioUrl,
    double DurationInSecond, List<SentenceVM>? SubtitleSentences)
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="episode">确定不为空的episode</param>
    /// <param name="loadSubtitle">是否加载字幕</param>
    /// <returns>若不可见，则return null</returns>
    public static EpisodeVM? Create(Episode episode, bool loadSubtitle)
    {
        if (episode.IsVisible == false)
        {
            return null;
        }
        if (loadSubtitle)
        {
            IEnumerable<SentenceVM> sentenceVMs = episode.Subtitle.ParseSubTitle()
                .Select(sentence => new SentenceVM(sentence.StartTime.TotalSeconds, sentence.EndTime.TotalSeconds, sentence.Value));
            EpisodeVM episodeVM = new(episode.Id, episode.Name, episode.AlbumId, episode.AudioUrl,
                episode.DurationInSecond, sentenceVMs.ToList());
            return episodeVM;
        }
        else
        {
            return new EpisodeVM(episode.Id, episode.Name, episode.AlbumId, episode.AudioUrl,
                episode.DurationInSecond, null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="episodes">列表中的每个Episode都不能为空</param>
    /// <param name="loadSubtitle">是否加载字幕</param>
    /// <returns></returns>
    public static List<EpisodeVM> Create(List<Episode> episodes, bool loadSubtitle)
    {
        //episodes.Select(episode => EpisodeVM.Create(episode))
        List<EpisodeVM> vmList = new();
        foreach (Episode episode in episodes)
        {
            EpisodeVM? vm = Create(episode, loadSubtitle);
            if (vm != null)
            {
                vmList.Add(vm);
            }
        }
        return vmList;
    }
}
