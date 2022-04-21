// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace CsvHelper
{
	/// <summary>
	/// Defines methods used to read parsed data
	/// from a CSV file.
	/// </summary>
	public interface IReader : IReaderRow, IDisposable
	{
		/// <summary>
		/// Reads the header record without reading the first row.
		/// </summary>
		/// <returns>True if there are more records, otherwise false.</returns>
		bool ReadHeader();

		/// <summary>
		/// Advances the reader to the next record. This will not read headers.
		/// You need to call <see cref="Read"/> then <see cref="ReadHeader"/> 
		/// for the headers to be read.
		/// </summary>
		/// <returns>True if there are more records, otherwise false.</returns>
		bool Read();

		/// <summary>
		/// Advances the reader to the next record. This will not read headers.
		/// You need to call <see cref="ReadAsync"/> then <see cref="ReadHeader"/> 
		/// for the headers to be read.
		/// </summary>
		/// <returns>True if there are more records, otherwise false.</returns>
		Task<bool> ReadAsync();
	}
}
