// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Globalization;

namespace CsvHelper.Configuration;

/// <summary>
/// Configuration used for the <see cref="IReader"/>.
/// </summary>
public interface IReaderConfiguration : IParserConfiguration
{
	/// <summary>
	/// Gets a value indicating if the
	/// CSV file has a header record.
	/// Default is true.
	/// </summary>
	bool HasHeaderRecord { get; }

	/// <summary>
	/// Gets the function that is called when a missing field is found. The default function will
	/// throw a <see cref="MissingFieldException"/>. You can supply your own function to do other things
	/// like logging the issue instead of throwing an exception.
	/// </summary>
	MissingFieldFound MissingFieldFound { get; }

	/// <summary>
	/// Gets the function that is called when a reading exception occurs.
	/// The default function will re-throw the given exception. If you want to ignore
	/// reading exceptions, you can supply your own function to do other things like
	/// logging the issue.
	/// </summary>
	ReadingExceptionOccurred ReadingExceptionOccurred { get; }

	/// <summary>
	/// Gets the culture info used to read an write CSV files.
	/// </summary>
	CultureInfo CultureInfo { get; }

	/// <summary>
	/// Prepares the header field for matching against a member name.
	/// The header field and the member name are both ran through this function.
	/// You should do things like trimming, removing whitespace, removing underscores,
	/// and making casing changes to ignore case.
	/// </summary>
	PrepareHeaderForMatch PrepareHeaderForMatch { get; }

	/// <summary>
	/// Gets the callback that will be called to
	/// determine whether to skip the given record or not.
	/// </summary>
	ShouldSkipRecord ShouldSkipRecord { get; }

	/// <summary>
	/// Gets a value indicating if private
	/// member should be read from and written to.
	/// <c>true</c> to include private member, otherwise <c>false</c>. Default is false.
	/// </summary>
	bool IncludePrivateMembers { get; }

	/// <summary>
	/// Gets a value indicating whether changes in the column
	/// count should be detected. If true, a <see cref="BadDataException"/>
	/// will be thrown if a different column count is detected.
	/// </summary>
	bool DetectColumnCountChanges { get; }
}
