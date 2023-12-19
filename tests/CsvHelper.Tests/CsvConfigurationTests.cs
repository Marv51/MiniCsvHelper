// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Globalization;
using CsvHelper.Configuration;

namespace CsvHelper.Tests;

public class CsvConfigurationTests
{
	[Fact]
	public void EnsureReaderAndParserConfigIsAreSameTest()
	{
		using var stream = new MemoryStream();
		using var reader = new StreamReader(stream);
		var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);

		Assert.Same(csvReader.Configuration, csvReader.Parser.Configuration);

		var config = new CsvConfiguration(CultureInfo.InvariantCulture);
		var parser = new CsvParser(reader, config);
		csvReader = new CsvReader(parser);

		Assert.Same(csvReader.Configuration, csvReader.Parser.Configuration);
	}

	private class TestClass
	{
		public string StringColumn { get; set; }
		public int IntColumn { get; set; }
	}
}
