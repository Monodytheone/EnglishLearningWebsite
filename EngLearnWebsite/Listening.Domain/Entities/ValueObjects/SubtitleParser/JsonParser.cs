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
            return JsonSerializer.Deserialize<IEnumerable<Sentence>>(content);
        }
    }
}
