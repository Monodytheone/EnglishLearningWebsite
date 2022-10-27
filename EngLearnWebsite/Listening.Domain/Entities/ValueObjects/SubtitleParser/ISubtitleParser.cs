using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listening.Domain.Entities.ValueObjects.SubtitleParser
{
    internal interface ISubtitleParser
    {
        /// <summary>
        /// 本解析器是否能解析format格式的字幕
        /// </summary>
        bool Accept(SubtitleType format);

        /// <summary>
        /// 解析为一个一个Sentence
        /// </summary>
        /// <param name="content">字幕原文</param>
        /// <returns></returns>
        IEnumerable<Sentence> Parse(string content);
    }
}
