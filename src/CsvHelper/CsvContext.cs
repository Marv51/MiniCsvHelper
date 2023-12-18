// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using CsvHelper.Configuration;

namespace CsvHelper;

/// <summary>
/// Share state for CsvHelper.
/// </summary>
public class CsvContext
    {
	/// <summary>
	/// Gets the parser.
	/// </summary>
	public IParser Parser { get; private set; }

	/// <summary>
	/// Gets the reader.
	/// </summary>
	public IReader Reader { get; internal set; }

	/// <summary>
	/// Gets the configuration.
	/// </summary>
	public CsvConfiguration Configuration { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvContext"/> class.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public CsvContext(IReader reader)
	{
		Reader = reader;
		Parser = reader.Parser;
		Configuration = reader.Configuration as CsvConfiguration ?? throw new InvalidOperationException($"{nameof(IReader)}.{nameof(IReader.Configuration)} must be of type {nameof(CsvConfiguration)} to be used in the context.");
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvContext"/> class.
	/// </summary>
	/// <param name="parser">The parser.</param>
	public CsvContext(IParser parser)
	{
		Parser = parser;
		Configuration = parser.Configuration as CsvConfiguration ?? throw new InvalidOperationException($"{nameof(IParser)}.{nameof(IParser.Configuration)} must be of type {nameof(CsvConfiguration)} to be used in the context.");
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvContext"/> class.
	/// </summary>
	/// <param name="configuration">The configuration.</param>
	public CsvContext(CsvConfiguration configuration)
	{
		Configuration = configuration;
	}
}
