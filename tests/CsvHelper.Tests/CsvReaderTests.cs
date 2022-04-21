// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CsvHelper.Configuration;
using CsvHelper.Tests.Mocks;
using Xunit;
#pragma warning disable 649

namespace CsvHelper.Tests
{
	
	public class CsvReaderTests
	{
		[Fact]
		public void GetMissingFieldByIndexStrictTest()
		{
			var parserMock = new ParserMock
			{
				{ "One", "Two" },
				{ "1", "2" },
				null,
			};

			var reader = new CsvReader(parserMock);
			reader.Read();

			Assert.Throws<MissingFieldException>(() => reader.GetField(2));
		}

		[Fact]
		public void GetMissingFieldByIndexStrictOffTest()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				MissingFieldFound = null,
			};
			var parserMock = new ParserMock(config)
			{
				{ "One", "Two" },
				{ "1", "2" },
				null,
			};

			var reader = new CsvReader(parserMock);
			reader.Read();

			Assert.Null(reader.GetField(2));
		}

		[Fact]
		public void GetRecordWithDuplicateHeaderFields()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				MissingFieldFound = null,
			};
			var parserMock = new ParserMock(config)
			{
				{ "Field1", "Field1" },
				{ "Field1", "Field1" },
			};

			var reader = new CsvReader(parserMock);
			reader.Read();
		}

		[Fact]
		public void GetRecordEmptyFileWithHeaderOnTest()
		{
			var parserMock = new ParserMock
			{
				null,
			};

			var csvReader = new CsvReader(parserMock);
			try
			{
				csvReader.Read();
				csvReader.ReadHeader();
				csvReader.Read();
				throw new XunitException();
			}
			catch (ReaderException) { }
		}


		[Fact]
		public void CaseInsensitiveHeaderMatchingTest()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				PrepareHeaderForMatch = args => args.Header.ToLower(),
			};
			using (var stream = new MemoryStream())
			using (var writer = new StreamWriter(stream))
			using (var reader = new StreamReader(stream))
			using (var csv = new CsvReader(reader, config))
			{
				writer.WriteLine("One,Two,Three");
				writer.WriteLine("1,2,3");
				writer.Flush();
				stream.Position = 0;

				csv.Read();
				csv.ReadHeader();
				csv.Read();

				Assert.Equal("1", csv.GetField("one"));
				Assert.Equal("2", csv.GetField("TWO"));
				Assert.Equal("3", csv.GetField("ThreE"));
			}
		}


		[Fact]
		public void SkipEmptyRecordsTest()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = false,
				ShouldSkipRecord = args => args.Record.All(string.IsNullOrWhiteSpace),
			};

			var parserMock = new ParserMock(config)
			{
				{ "1", "2", "3" },
				{ "", "", "" },
				{ "4", "5", "6" },
			};

			var reader = new CsvReader(parserMock);

			reader.Read();
			Assert.Equal("1", reader.Parser.Record[0]);
			Assert.Equal("2", reader.Parser.Record[1]);
			Assert.Equal("3", reader.Parser.Record[2]);

			reader.Read();
			Assert.Equal("4", reader.Parser.Record[0]);
			Assert.Equal("5", reader.Parser.Record[1]);
			Assert.Equal("6", reader.Parser.Record[2]);

			Assert.False(reader.Read());
		}

		[Fact]
		public void SkipRecordCallbackTest()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = false,
				ShouldSkipRecord = args => args.Record[1] == "2",
			};

			var parserMock = new ParserMock(config)
			{
				{ "1", "2", "3" },
				{ " ", "", "" },
				{ "4", "5", "6" },
			};

			var reader = new CsvReader(parserMock);

			reader.Read();
			Assert.Equal(" ", reader.Parser.Record[0]);
			Assert.Equal("", reader.Parser.Record[1]);
			Assert.Equal("", reader.Parser.Record[2]);

			reader.Read();
			Assert.Equal("4", reader.Parser.Record[0]);
			Assert.Equal("5", reader.Parser.Record[1]);
			Assert.Equal("6", reader.Parser.Record[2]);

			Assert.False(reader.Read());
		}

		[Fact]
		public void TrimHeadersTest()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				MissingFieldFound = null,
				PrepareHeaderForMatch = args => args.Header.Trim(),
			};
			var parserMock = new ParserMock(config)
			{
				{ " one ", " two three " },
				{ "1", "2" },
			};
			var reader = new CsvReader(parserMock);
			reader.Read();
			reader.ReadHeader();
			reader.Read();
			Assert.Equal("1", reader.GetField("one"));
			Assert.Equal("2", reader.GetField("two three"));
			Assert.Null(reader.GetField("twothree"));
		}

		[Fact]
		public void RowTest()
		{
			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = false,
			};
			var parserMock = new ParserMock(config)
			{
				{ "1", "one" },
				{ "2", "two" },
			};

			var csv = new CsvReader(parserMock);

			csv.Read();
			Assert.Equal(1, csv.Parser.Row);

			csv.Read();
			Assert.Equal(2, csv.Parser.Row);
		}

		private class Nested
		{
			public Simple Simple1 { get; set; }

			public Simple Simple2 { get; set; }
		}

		private class Simple
		{
			public int? Id { get; set; }

			public string Name { get; set; }
		}

		private class TestStructParent
		{
			public TestStruct Test { get; set; }
		}

		private struct TestStruct
		{
			public int Id { get; set; }

			public string Name { get; set; }
		}

		private class OnlyFields
		{
			public string Name;
		}

		private class TestBoolean
		{
			public bool BoolColumn { get; set; }

			public bool BoolNullableColumn { get; set; }

			public string StringColumn { get; set; }
		}

		private class TestDefaultValues
		{
			public int IntColumn { get; set; }

			public string StringColumn { get; set; }
		}

		private class TestNullable
		{
			public int? IntColumn { get; set; }

			public string StringColumn { get; set; }

			public Guid? GuidColumn { get; set; }
		}

		[DebuggerDisplay("IntColumn = {IntColumn}, StringColumn = {StringColumn}, IgnoredColumn = {IgnoredColumn}, TypeConvertedColumn = {TypeConvertedColumn}, FirstColumn = {FirstColumn}")]
		private class TestRecord
		{
			public int IntColumn { get; set; }

			public string StringColumn { get; set; }

			public string IgnoredColumn { get; set; }

			public string TypeConvertedColumn { get; set; }

			public int FirstColumn { get; set; }

			public Guid GuidColumn { get; set; }

			public int NoMatchingFields { get; set; }
		}

		private class TestRecordDuplicateHeaderNames
		{
			public string Column1 { get; set; }

			public string Column2 { get; set; }

			public string Column3 { get; set; }
		}
	}
}
