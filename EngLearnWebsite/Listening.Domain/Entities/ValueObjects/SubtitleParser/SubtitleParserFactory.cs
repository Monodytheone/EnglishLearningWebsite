namespace Listening.Domain.Entities.ValueObjects.SubtitleParser;

internal static class SubtitleParserFactory
{
    private static List<ISubtitleParser> _parsers = new();

	static SubtitleParserFactory()
	{
        // 扫描本程序集中的所有实现了ISubtitleParser接口的类
        IEnumerable<Type> parserTypes = typeof(SubtitleParserFactory).Assembly.GetTypes()
			.Where(t => typeof(ISubtitleParser).IsAssignableFrom(t) && t.IsAbstract == false);

        //创建这些对象，添加到_parsers
        foreach (Type parserType in parserTypes)
		{
			ISubtitleParser parser = (ISubtitleParser)Activator.CreateInstance(parserType)!;
			_parsers.Add(parser);
		}
	}

	public static ISubtitleParser GetParser(SubtitleType format)
	{
		// 遍历所有解析器，挨个问他们“能解析这个格式吗”，碰到一个能解析的，就会把解析器返回
		foreach (ISubtitleParser parser in _parsers)
		{
			if (parser.Accept(format) == true)
			{
				return parser;
			}
		}
		throw new ApplicationException("竟然没找到对应的Parser!");
    }
}
