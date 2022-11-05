using SearchService.Domain;
using Zack.EventBus;

namespace SearchService.WebAPI.EventHandlers;

[EventName("Listening.Episode.SoftDelete")]
[EventName("Listening.Episode.Hide")]  // 被隐藏也要从搜索服务中删除
public class LIsteningEpisodeDeleteEventHandler : DynamicIntegrationEventHandler
{
    private readonly ISearchRepository _repository;

    public LIsteningEpisodeDeleteEventHandler(ISearchRepository repository)
    {
        _repository = repository;
    }

    public override Task HandleDynamic(string eventName, dynamic eventData)
    {
        Guid id = Guid.Parse(eventData.Id);
        return _repository.DeleteAsync(id);
    }
}
