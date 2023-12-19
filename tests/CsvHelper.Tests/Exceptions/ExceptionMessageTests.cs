// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Tests.Mocks;

namespace CsvHelper.Tests.Exceptions;

public class ExceptionMessageTests
{
	[Fact]
	public void GetMissingFieldTest()
	{
		var parser = new ParserMock
		{
			{ "Id", "Name" },
			{ "a", "b" },
			null
		};

		var reader = new CsvReader(parser);
		reader.Read();
		reader.Read();
		try
		{
			reader.GetField(2);
			throw new XunitException();
		}
		catch (MissingFieldException ex)
		{
			Assert.Equal(2, ex.Context.Parser.Row);
			Assert.Equal(2, ex.Context.Reader.CurrentIndex);
		}
	}

	[Fact]
	public void GetFieldIndexTest()
	{
		var parser = new ParserMock
		{
			{ "Id", "Name" },
			{ "a", "b" },
			null
		};

		var reader = new CsvReader(parser);
		reader.Read();
		reader.ReadHeader();
		reader.Read();

		try
		{
			reader.GetField("c");
			throw new XunitException();
		}
		catch (MissingFieldException ex)
		{
			Assert.Equal(2, ex.Context.Parser.Row);
			Assert.Equal(-1, ex.Context.Reader.CurrentIndex);
		}
	}

	private class Simple
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}
}
