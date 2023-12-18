// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System.Globalization;

namespace CsvHelper.Configuration;

/// <summary>
/// The Write Configuration
/// </summary>
public interface IWriterConfiguration
{
	/// <summary>
	/// The mode.
	/// See <see cref="CsvMode"/> for more details.
	/// </summary>
	CsvMode Mode { get; }

	/// <summary>
	/// A value indicating whether to leave the <see cref="TextReader"/> or <see cref="TextWriter"/> open after this object is disposed.
	/// </summary>
	/// <value>
	///   <c>true</c> to leave open, otherwise <c>false</c>.
	/// </value>
	bool LeaveOpen { get; }

	/// <summary>
	/// Gets the delimiter used to separate fields.
	/// Default is ',';
	/// </summary>
	string Delimiter { get; }

	/// <summary>
	/// Gets the character used to quote fields.
	/// Default is '"'.
	/// </summary>
	char Quote { get; }

	/// <summary>
	/// The character used to escape characters.
	/// Default is '"'.
	/// </summary>
	char Escape { get; }

	/// <summary>
	/// Gets the field trimming options.
	/// </summary>
	TrimOptions TrimOptions { get; }

	/// <summary>
	/// The newline string to use. Default is \r\n (CRLF).
	/// When writing, this value is always used.
	/// When reading, this value is only used if explicitly set. If not set,
	/// the parser uses one of \r\n, \r, or \n.
	/// </summary>
	string NewLine { get; }

	/// <summary>
	/// A value indicating if <see cref="NewLine"/> was set.
	/// </summary>
	/// <value>
	///   <c>true</c> if <see cref="NewLine"/> was set. <c>false</c> if <see cref="NewLine"/> is the default.
	/// </value>
	bool IsNewLineSet { get; }

	/// <summary>
	/// Gets the culture info used to read and write CSV files.
	/// </summary>
	CultureInfo CultureInfo { get; }

	/// <summary>
	/// Gets a value indicating if comments are allowed.
	/// True to allow commented out lines, otherwise false.
	/// </summary>
	bool AllowComments { get; }

	/// <summary>
	/// Gets the character used to denote
	/// a line that is commented out. Default is '#'.
	/// </summary>
	char Comment { get; }

	/// <summary>
	/// Gets a value indicating if the
	/// CSV file has a header record.
	/// Default is true.
	/// </summary>
	bool HasHeaderRecord { get; }

	/// <summary>
	/// A value indicating if exception messages contain raw CSV data.
	/// <c>true</c> if exception contain raw CSV data, otherwise <c>false</c>.
	/// Default is <c>true</c>.
	/// </summary>
	bool ExceptionMessagesContainRawData { get; }
}
