using Listening.Domain.Entities.ValueObjects.SubtitleParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listening.Domain.Entities.ValueObjects
{
    /// <summary>
    /// 字幕格式
    /// </summary>
    public enum SubtitleType
    {
        Json,
        lrc,
        srt,
        vtt,
    }
    

    public class Subtitle
    {
        public string Content { get; private set; }

        public SubtitleType Format { get; private set; }

        public Subtitle() { }  // 这个不改成public的，有个地方读不到（具体哪里我忘了。。。）

        public Subtitle(string content, SubtitleType type)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Format = type;
        }

        public Subtitle(string content, string type)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Format = Enum.Parse<SubtitleType>(type);
        }

        public Subtitle ChangeContent(string newContent)
        {
            this.Content = newContent;
            return this;
        }

        public Subtitle ChangeFormat(SubtitleType newValue)
        {
            this.Format = newValue;
            return this;
        }

        public IEnumerable<Sentence> ParseSubTitle()
        {
            ISubtitleParser parser = SubtitleParserFactory.GetParser(Format);
            return parser.Parse(Content);
        }
    }
}
