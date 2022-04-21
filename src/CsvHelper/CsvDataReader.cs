// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System;
using System.Data;
using System.Globalization;
using System.Linq;

namespace CsvHelper
{
	/// <summary>
	/// Provides a means of reading a CSV file forward-only by using CsvReader.
	/// </summary>
	/// <seealso cref="System.Data.IDataReader" />
	public class CsvDataReader
	{
		private readonly CsvReader csv;
		private bool skipNextRead;

		/// <summary>
		/// Gets the column with the specified index.
		/// </summary>
		/// <value>
		/// The <see cref="System.Object"/>.
		/// </value>
		/// <param name="i">The i.</param>
		/// <returns></returns>
		public object this[int i]
		{
			get
			{
				return csv[i];
			}
		}

		/// <summary>
		/// Gets the column with the specified name.
		/// </summary>
		/// <value>
		/// The <see cref="System.Object"/>.
		/// </value>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public object this[string name]
		{
			get
			{
				return csv[name];
			}
		}

		/// <summary>
		/// Gets a value indicating whether the data reader is closed.
		/// </summary>
		public bool IsClosed { get; private set; }

		/// <summary>
		/// Gets the number of columns in the current row.
		/// </summary>
		public int FieldCount
		{
			get
			{
				return csv?.Parser.Count ?? 0;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CsvDataReader"/> class.
		/// </summary>
		/// <param name="csv">The CSV.</param>
		public CsvDataReader(CsvReader csv)
		{
			this.csv = csv;

			csv.Read();

			if (csv.Configuration.HasHeaderRecord)
			{
				csv.ReadHeader();
			}
			else
			{
				skipNextRead = true;
			}
		}

		/// <summary>
		/// Closes the <see cref="T:System.Data.IDataReader"></see> Object.
		/// </summary>
		public void Close()
		{
			Dispose();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			csv.Dispose();
			IsClosed = true;
		}

		/// <summary>
		/// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
		/// </summary>
		/// <param name="i">The zero-based column ordinal.</param>
		/// <param name="fieldoffset">The index within the row from which to start the read operation.</param>
		/// <param name="buffer">The buffer into which to read the stream of bytes.</param>
		/// <param name="bufferoffset">The index for buffer to start the read operation.</param>
		/// <param name="length">The number of bytes to read.</param>
		/// <returns>
		/// The actual number of characters read.
		/// </returns>
		public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
		{
			var chars = csv.GetField(i).ToCharArray();

			Array.Copy(chars, fieldoffset, buffer, bufferoffset, length);

			return chars.Length;
		}

		/// <summary>
		/// Gets the name for the field to find.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The name of the field or the empty string (""), if there is no value to return.
		/// </returns>
		public string GetName(int i)
		{
			return csv.Configuration.HasHeaderRecord
				? csv.HeaderRecord[i]
				: string.Empty;
		}

		/// <summary>
		/// Return the index of the named field.
		/// </summary>
		/// <param name="name">The name of the field to find.</param>
		/// <returns>
		/// The index of the named field.
		/// </returns>
		public int GetOrdinal(string name)
		{
			var index = csv.GetFieldIndex(name, isTryGet: true);
			if (index >= 0)
			{
				return index;
			}

			var args = new PrepareHeaderForMatchArgs(name, 0);
			var namePrepared = csv.Configuration.PrepareHeaderForMatch(args);

			var headerRecord = csv.HeaderRecord;
			for (var i = 0; i < headerRecord.Length; i++)
			{
				args = new PrepareHeaderForMatchArgs(headerRecord[i], i);
				var headerPrepared = csv.Configuration.PrepareHeaderForMatch(args);
				if (csv.Configuration.CultureInfo.CompareInfo.Compare(namePrepared, headerPrepared, CompareOptions.IgnoreCase) == 0)
				{
					return i;
				}
			}

			throw new IndexOutOfRangeException($"Field with name '{name}' and prepared name '{namePrepared}' was not found.");
		}

		/// <summary>
		/// Gets the string value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The string value of the specified field.
		/// </returns>
		public string GetString(int i)
		{
			return csv.GetField(i);
		}

		/// <summary>
		/// Return the value of the specified field.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// The <see cref="T:System.Object"></see> which will contain the field value upon return.
		/// </returns>
		public object GetValue(int i)
		{
			return IsDBNull(i) ? DBNull.Value : (object)csv.GetField(i);
		}

		/// <summary>
		/// Populates an array of objects with the column values of the current record.
		/// </summary>
		/// <param name="values">An array of <see cref="T:System.Object"></see> to copy the attribute fields into.</param>
		/// <returns>
		/// The number of instances of <see cref="T:System.Object"></see> in the array.
		/// </returns>
		public int GetValues(object[] values)
		{
			for (var i = 0; i < values.Length; i++)
			{
				values[i] = IsDBNull(i) ? DBNull.Value : (object)csv.GetField(i);
			}

			return csv.Parser.Count;
		}

		/// <summary>
		/// Return whether the specified field is set to null.
		/// </summary>
		/// <param name="i">The index of the field to find.</param>
		/// <returns>
		/// true if the specified field is set to null; otherwise, false.
		/// </returns>
		public bool IsDBNull(int i)
		{
			var field = csv.GetField(i);
			var nullValues = new string[] { };

			return nullValues.Contains(field);
		}

		/// <summary>
		/// Advances the data reader to the next result, when reading the results of batch SQL statements.
		/// </summary>
		/// <returns>
		/// true if there are more rows; otherwise, false.
		/// </returns>
		public bool NextResult()
		{
			return false;
		}

		/// <summary>
		/// Advances the <see cref="T:System.Data.IDataReader"></see> to the next record.
		/// </summary>
		/// <returns>
		/// true if there are more rows; otherwise, false.
		/// </returns>
		public bool Read()
		{
			if (skipNextRead)
			{
				skipNextRead = false;
				return true;
			}

			return csv.Read();
		}
	}
}
