using Opportunity.LrcParser;

namespace Listening.Domain.Entities.ValueObjects.SubtitleParser;

internal class LrcParser : ISubtitleParser
{
    public bool Accept(SubtitleType format)
    {
        return format == SubtitleType.lrc;
    }

    public IEnumerable<Sentence> Parse(string content)
    {
        IParseResult<Line> lyrics = Lyrics.Parse(content);
        if (lyrics.Exceptions.Count > 0)
        {
            throw new ApplicationException("lrc解析失败");
        }
        lyrics.Lyrics.PreApplyOffset();//应用上[offset:500]这样的偏移
        return FromLrc(lyrics.Lyrics);
    }

    private static Sentence[] FromLrc(Lyrics<Line> lyrics)
    {
        var lines = lyrics.Lines;
        Sentence[] sentences = new Sentence[lines.Count];
        for (int i = 0; i < lines.Count - 1; i++)
        {
            var line = lines[i];
            var nextLine = lines[i + 1];
            Sentence sentence = new(line.Timestamp.TimeOfDay, nextLine.Timestamp.TimeOfDay, line.Content);
            sentences[i] = sentence;
        }

        var lastLine = lines.Last();
        TimeSpan lastLineStartTime = lastLine.Timestamp.TimeOfDay;
        //lrc没有结束时间，就极端假定最后一句耗时1分钟
        TimeSpan lastLineEndTime = lastLineStartTime.Add(TimeSpan.FromMinutes(1));
        Sentence lastSentence = new(lastLineStartTime, lastLineEndTime, lastLine.Content);
        sentences[sentences.Count() - 1] = lastSentence;

        return sentences;
    }
}
