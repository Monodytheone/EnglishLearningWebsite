using System.Text;

namespace Listening.Domain.Entities.ValueObjects.SubtitleParser;

/// <summary>
/// *.srt和*.vtt字幕文件解析器
/// </summary>
internal class SrtParser : ISubtitleParser
{
    public bool Accept(SubtitleType format)
    {
        return format == SubtitleType.srt || format == SubtitleType.vtt;
    }

    public IEnumerable<Sentence> Parse(string content)
    {
        var srtParser = new SubtitlesParser.Classes.Parsers.SubParser();
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
        {
            var items = srtParser.ParseStream(ms);
            return items.Select(s => new Sentence(TimeSpan.FromMilliseconds(s.StartTime), TimeSpan.FromMilliseconds(s.EndTime), String.Join(" ", s.Lines)));
        }
    }
}
