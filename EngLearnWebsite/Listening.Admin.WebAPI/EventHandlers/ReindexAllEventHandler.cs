using Listening.Domain.Entities;
using Listening.Domain.Entities.ValueObjects;
using Listening.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Zack.EventBus;

namespace Listening.Admin.WebAPI.EventHandlers;

[EventName("SearchService.ReIndexAll")]
public class ReindexAllEventHandler : IIntegrationEventHandler
{
    private readonly ListeningDbContext _dbContext;
    private readonly IEventBus _eventBus;

    public ReindexAllEventHandler(ListeningDbContext dbContext, IEventBus eventBus)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
    }

    public Task Handle(string eventName, string eventData)
    {
        foreach (Episode episode in _dbContext.Query<Episode>())
        {
            if (episode.IsVisible)
            {
                IEnumerable<Sentence> sentences = episode.Subtitle.ParseSubTitle();
                _eventBus.Publish("Listening.Episode.Updated", new { Id = episode.Id, episode.Name, Sentences = sentences, episode.AlbumId, episode.Subtitle });
            }
        }
        return Task.CompletedTask;
    }
}
