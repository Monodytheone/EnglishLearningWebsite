using SearchService.Domain;
using Zack.EventBus;

namespace SearchService.WebAPI.EventHandlers;

[EventName("Listening.Episode.Created")]
[EventName("Listening.Episode.Updated")]
public class ListeningEpisodeUpsertEventHandler : DynamicIntegrationEventHandler
{
    private readonly ISearchRepository _repository;

    public ListeningEpisodeUpsertEventHandler(ISearchRepository repository)
    {
        _repository = repository;
    }

    public override Task HandleDynamic(string eventName, dynamic eventData)
    {
        try
        {
            Guid id = Guid.Parse(eventData.Id);
            string cnName = eventData.Name.Chinese;
            string engName = eventData.Name.English;
            Guid albumId = Guid.Parse(eventData.AlbumId);
            List<string> sentences = new();
            foreach (string sentence in eventData.Sentences)
            {
                sentences.Add(sentence);
            }
            string plainSentences = string.Join("\r\n", sentences);
            EpisodeInfo episodeInfo = new(id, cnName, engName, plainSentences, albumId);
            return _repository.UpsertAsync(episodeInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine("可能是集成事件dynamic的解析出问题了:");
            Console.WriteLine(ex);
            return Task.CompletedTask;
        }
    }
}
