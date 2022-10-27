using Listening.Domain.Entities;
using Listening.Domain.Entities.ValueObjects;
using Listening.Domain.Notifications;
using MediatR;
using Zack.EventBus;

namespace Listening.Admin.WebAPI.NotificationHandlers;

public class EpisodeCreateNotificationHandler : INotificationHandler<EpisodeCreateNotification>
{
    private readonly IEventBus _eventBus;

    public EpisodeCreateNotificationHandler(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public Task Handle(EpisodeCreateNotification notification, CancellationToken cancellationToken)
    {
        // 把领域事件转发为集成事件，让其他微服务听到
        Episode episode = notification.CreatedEpisode;
        IEnumerable<Sentence> sentences = episode.Subtitle.ParseSubTitle();
        _eventBus.Publish("Listening.Episode.Created", new { Id = episode.Id, Name = episode.Name, Sentences = sentences, episode.AlbumId, episode.Subtitle });
        return Task.CompletedTask;
    }
}
