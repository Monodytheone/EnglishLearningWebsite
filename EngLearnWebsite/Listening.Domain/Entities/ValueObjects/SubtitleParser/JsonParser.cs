using System.Text.Json;

namespace Listening.Domain.Entities.ValueObjects.SubtitleParser
{
    internal class JsonParser : ISubtitleParser
    {
        public bool Accept(SubtitleType format)
        {
            return format == SubtitleType.Json;
        }

        public IEnumerable<Sentence> Parse(string content)
        {
            if(content == null)  // 因为redis总把字幕内容存成null，格式存成0（Json），我暂时妥协了，随便给个不为null的非法值的了
            {
                var sentences = new List<Sentence>();
                Sentence sentence = new(TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(1), "字幕处理失败了哦");
                sentences.Add(sentence);
                return sentences;
            }
            return JsonSerializer.Deserialize<IEnumerable<Sentence>>(content);
        }
    }
}
