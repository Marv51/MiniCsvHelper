// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper

namespace CsvHelper;

/// <summary>
/// Represents errors that occur due to bad data.
/// </summary>
public class BadDataException : CsvHelperException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="BadDataException"/> class
	/// with a specified error message.
	/// </summary>
	/// <param name="context">The reading context.</param>
	/// <param name="message">The message that describes the error.</param>
	public BadDataException(CsvContext context, string message) : base(context, message) { }
}
