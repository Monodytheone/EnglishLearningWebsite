using Listening.Domain.Entities;
using Listening.Domain.Entities.ValueObjects;
using Listening.Domain.Notifications;
using MediatR;
using Zack.EventBus;

namespace Listening.Admin.WebAPI.NotificationHandlers
{
    public class EpisodeUpdateNotificationHandler : INotificationHandler<EpisodeUpdateNotification>
    {
        private readonly IEventBus _eventBus;

        public EpisodeUpdateNotificationHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public Task Handle(EpisodeUpdateNotification notification, CancellationToken cancellationToken)
        {
            Episode episode = notification.UpdatedEpisode;
            if (episode.IsVisible == true)
            {
                IEnumerable<Sentence> sentences = episode.Subtitle.ParseSubTitle();
                _eventBus.Publish("Listening.Episode.Updated", new { Id = episode.Id, Name = episode.Name, Sentences = sentences, episode.AlbumId, episode.Subtitle });
            }
            else
            {
                _eventBus.Publish("Listening.Episode.Hide", new { Id = episode.Id });
            }
            return Task.CompletedTask;
        }
    }
}
