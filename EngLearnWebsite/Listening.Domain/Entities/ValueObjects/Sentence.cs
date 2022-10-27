using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listening.Domain.Entities.ValueObjects
{
    /// <summary>
    /// 字幕的一句
    /// </summary>
    public record Sentence(TimeSpan StartTime, TimeSpan EndTime, string Value);
}
