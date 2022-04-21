// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System;
using CsvHelper.Configuration;

namespace CsvHelper
{
	/// <summary>
	/// Defines methods used to read parsed data
	/// from a CSV file row.
	/// </summary>
	public interface IReaderRow
	{
		/// <summary>
		/// Gets the column count of the current row.
		/// This should match <see cref="IParser.Count"/>.
		/// </summary>
		int ColumnCount { get; }

		/// <summary>
		/// Gets the field index the reader is currently on.
		/// </summary>
		int CurrentIndex { get; }

		/// <summary>
		/// Gets the header record.
		/// </summary>
		string[] HeaderRecord { get; }

		/// <summary>
		/// Gets the parser.
		/// </summary>
		IParser Parser { get; }

		/// <summary>
		/// Gets the reading context.
		/// </summary>
		CsvContext Context { get; }

		/// <summary>
		/// Gets or sets the configuration.
		/// </summary>
		IReaderConfiguration Configuration { get; }

		/// <summary>
		/// Gets the raw field at position (column) index.
		/// </summary>
		/// <param name="index">The zero based index of the field.</param>
		/// <returns>The raw field.</returns>
		string this[int index] { get; }

		/// <summary>
		/// Gets the raw field at position (column) name.
		/// </summary>
		/// <param name="name">The named index of the field.</param>
		/// <returns>The raw field.</returns>
		string this[string name] { get; }

		/// <summary>
		/// Gets the raw field at position (column) name.
		/// </summary>
		/// <param name="name">The named index of the field.</param>
		/// <param name="index">The zero based index of the field.</param>
		/// <returns>The raw field.</returns>
		string this[string name, int index] { get; }

		/// <summary>
		/// Gets the raw field at position (column) index.
		/// </summary>
		/// <param name="index">The zero based index of the field.</param>
		/// <returns>The raw field.</returns>
		string GetField(int index);

		/// <summary>
		/// Gets the raw field at position (column) name.
		/// </summary>
		/// <param name="name">The named index of the field.</param>
		/// <returns>The raw field.</returns>
		string GetField(string name);

		/// <summary>
		/// Gets the raw field at position (column) name and the index
		/// instance of that field. The index is used when there are
		/// multiple columns with the same header name.
		/// </summary>
		/// <param name="name">The named index of the field.</param>
		/// <param name="index">The zero based index of the instance of the field.</param>
		/// <returns>The raw field.</returns>
		string GetField(string name, int index);
	}
}
