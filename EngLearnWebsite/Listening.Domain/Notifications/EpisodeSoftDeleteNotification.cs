using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listening.Domain.Notifications
{
    public record EpisodeSoftDeleteNotification(Guid EpisodeId) : INotification;
}
