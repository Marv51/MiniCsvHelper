using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace CsvHelper.Tests.Parsing;

public class BufferSplittingEscapeAndQuoteTests
{
	[Fact]
	public void Read_BufferEndsAtEscape_FieldIsNotBadData()
	{
		var s = new StringBuilder();
		s.Append("a,\"bcdefghijklm\"\"nopqrstuvwxyz\"\r\n");
		var config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			BufferSize = 16,
		};
		using var reader = new StringReader(s.ToString());
		using var parser = new CsvParser(reader, config);
		parser.Read();
		Assert.Equal("a", parser[0]);
		Assert.Equal("bcdefghijklm\"nopqrstuvwxyz", parser[1]);
	}
}
