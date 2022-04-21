// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper
using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

namespace CsvHelper
{
	/// <summary>
	/// Reads data that was parsed from <see cref="IParser" />.
	/// </summary>
	public class CsvReader : IReader
	{
		private readonly bool detectColumnCountChanges;
		private readonly Dictionary<string, List<int>> namedIndexes = new Dictionary<string, List<int>>();
		private readonly Dictionary<string, (string, int)> namedIndexCache = new Dictionary<string, (string, int)>();
		private readonly bool hasHeaderRecord;
		private readonly ShouldSkipRecord shouldSkipRecord;
		private readonly ReadingExceptionOccurred readingExceptionOccurred;
		private readonly CultureInfo cultureInfo;
		private readonly bool ignoreBlankLines;
		private readonly MissingFieldFound missingFieldFound;
		private readonly bool includePrivateMembers;
		private readonly PrepareHeaderForMatch prepareHeaderForMatch;

		private CsvContext context;
		private bool disposed;
		private IParser parser;
		private int columnCount;
		private int currentIndex = -1;
		private bool hasBeenRead;
		private string[] headerRecord;

		/// <inheritdoc/>
		public virtual int ColumnCount => columnCount;

		/// <inheritdoc/>
		public virtual int CurrentIndex => currentIndex;

		/// <inheritdoc/>
		public virtual string[] HeaderRecord => headerRecord;

		/// <inheritdoc/>
		public virtual CsvContext Context => context;

		/// <inheritdoc/>
		public virtual IReaderConfiguration Configuration { get; private set; }

		/// <inheritdoc/>
		public virtual IParser Parser => parser;

		/// <summary>
		/// Creates a new CSV reader using the given <see cref="TextReader" />.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="culture">The culture.</param>
		/// <param name="leaveOpen"><c>true</c> to leave the <see cref="TextReader"/> open after the <see cref="CsvReader"/> object is disposed, otherwise <c>false</c>.</param>
		public CsvReader(TextReader reader, CultureInfo culture, bool leaveOpen = false) : this(new CsvParser(reader, culture, leaveOpen)) { }

		/// <summary>
		/// Creates a new CSV reader using the given <see cref="TextReader" /> and
		/// <see cref="CsvHelper.Configuration.CsvConfiguration" /> and <see cref="CsvParser" /> as the default parser.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="configuration">The configuration.</param>
		public CsvReader(TextReader reader, CsvConfiguration configuration) : this(new CsvParser(reader, configuration)) { }

		/// <summary>
		/// Creates a new CSV reader using the given <see cref="IParser" />.
		/// </summary>
		/// <param name="parser">The <see cref="IParser" /> used to parse the CSV file.</param>
		public CsvReader(IParser parser)
		{
			Configuration = parser.Configuration as IReaderConfiguration ?? throw new ConfigurationException($"The {nameof(IParser)} configuration must implement {nameof(IReaderConfiguration)} to be used in {nameof(CsvReader)}.");

			this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
			context = parser.Context ?? throw new InvalidOperationException($"For {nameof(IParser)} to be used in {nameof(CsvReader)}, {nameof(IParser.Context)} must also implement {nameof(CsvContext)}.");
			context.Reader = this;

			cultureInfo = Configuration.CultureInfo;
			detectColumnCountChanges = Configuration.DetectColumnCountChanges;
			hasHeaderRecord = Configuration.HasHeaderRecord;
			ignoreBlankLines = Configuration.IgnoreBlankLines;
			includePrivateMembers = Configuration.IncludePrivateMembers;
			missingFieldFound = Configuration.MissingFieldFound;
			prepareHeaderForMatch = Configuration.PrepareHeaderForMatch;
			readingExceptionOccurred = Configuration.ReadingExceptionOccurred;
			shouldSkipRecord = Configuration.ShouldSkipRecord;
		}

		/// <inheritdoc/>
		public virtual bool ReadHeader()
		{
			if (!hasHeaderRecord)
			{
				throw new ReaderException(context, "Configuration.HasHeaderRecord is false.");
			}

			headerRecord = parser.Record;
			ParseNamedIndexes();

			return headerRecord != null;
		}

		/// <inheritdoc/>
		public virtual bool Read()
		{
			// Don't forget about the async method below!

			bool hasMoreRecords;
			do
			{
				hasMoreRecords = parser.Read();
			}
			while (hasMoreRecords && shouldSkipRecord(new ShouldSkipRecordArgs(parser.Record)));

			currentIndex = -1;
			hasBeenRead = true;

			if (detectColumnCountChanges && hasMoreRecords)
			{
				if (columnCount > 0 && columnCount != parser.Count)
				{
					var csvException = new BadDataException(context, "An inconsistent number of columns has been detected.");

					var args = new ReadingExceptionOccurredArgs(csvException);
					if (readingExceptionOccurred?.Invoke(args) ?? true)
					{
						throw csvException;
					}
				}

				columnCount = parser.Count;
			}

			return hasMoreRecords;
		}

		/// <inheritdoc/>
		public virtual async Task<bool> ReadAsync()
		{
			bool hasMoreRecords;
			do
			{
				hasMoreRecords = await parser.ReadAsync();
			}
			while (hasMoreRecords && shouldSkipRecord(new ShouldSkipRecordArgs(parser.Record)));

			currentIndex = -1;
			hasBeenRead = true;

			if (detectColumnCountChanges && hasMoreRecords)
			{
				if (columnCount > 0 && columnCount != parser.Count)
				{
					var csvException = new BadDataException(context, "An inconsistent number of columns has been detected.");

					var args = new ReadingExceptionOccurredArgs(csvException);
					if (readingExceptionOccurred?.Invoke(args) ?? true)
					{
						throw csvException;
					}
				}

				columnCount = parser.Count;
			}

			return hasMoreRecords;
		}

		/// <inheritdoc/>
		public virtual string this[int index]
		{
			get
			{
				CheckHasBeenRead();

				return GetField(index);
			}
		}

		/// <inheritdoc/>
		public virtual string this[string name]
		{
			get
			{
				CheckHasBeenRead();

				return GetField(name);
			}
		}

		/// <inheritdoc/>
		public virtual string this[string name, int index]
		{
			get
			{
				CheckHasBeenRead();

				return GetField(name, index);
			}
		}

		/// <inheritdoc/>
		public virtual string GetField(int index)
		{
			CheckHasBeenRead();

			// Set the current index being used so we
			// have more information if an error occurs
			// when reading records.
			currentIndex = index;

			if (index >= parser.Count || index < 0)
			{
				if (ignoreBlankLines)
				{
					var args = new MissingFieldFoundArgs(null, index, context);
					missingFieldFound?.Invoke(args);
				}

				return default;
			}

			var field = parser[index];

			return field;
		}

		/// <inheritdoc/>
		public virtual string GetField(string name)
		{
			CheckHasBeenRead();

			var index = GetFieldIndex(name);
			if (index < 0)
			{
				return null;
			}

			return GetField(index);
		}

		/// <inheritdoc/>
		public virtual string GetField(string name, int index)
		{
			CheckHasBeenRead();

			var fieldIndex = GetFieldIndex(name, index);
			if (fieldIndex < 0)
			{
				return null;
			}

			return GetField(fieldIndex);
		}


		/// <inheritdoc/>
		public virtual int GetFieldIndex(string name, int index = 0, bool isTryGet = false)
		{
			return GetFieldIndex(new[] { name }, index, isTryGet);
		}

		/// <inheritdoc/>
		public virtual int GetFieldIndex(string[] names, int index = 0, bool isTryGet = false, bool isOptional = false)
		{
			if (names == null)
			{
				throw new ArgumentNullException(nameof(names));
			}

			if (!hasHeaderRecord)
			{
				throw new ReaderException(context, "There is no header record to determine the index by name.");
			}

			if (headerRecord == null)
			{
				throw new ReaderException(context, "The header has not been read. You must call ReadHeader() before any fields can be retrieved by name.");
			}

			// Caching the named index speeds up mappings that use ConvertUsing tremendously.
			var nameKey = string.Join("_", names) + index;
			if (namedIndexCache.ContainsKey(nameKey))
			{
				(var cachedName, var cachedIndex) = namedIndexCache[nameKey];
				return namedIndexes[cachedName][cachedIndex];
			}

			// Check all possible names for this field.
			string name = null;
			for (var i = 0; i < names.Length; i++)
			{
				var n = names[i];
				// Get the list of indexes for this name.
				var args = new PrepareHeaderForMatchArgs(n, i);
				var fieldName = prepareHeaderForMatch(args);
				if (namedIndexes.ContainsKey(fieldName))
				{
					name = fieldName;
					break;
				}
			}

			// Check if the index position exists.
			if (name == null || index >= namedIndexes[name].Count)
			{
				// It doesn't exist. The field is missing.
				if (!isTryGet && !isOptional)
				{
					var args = new MissingFieldFoundArgs(names, index, context);
					missingFieldFound?.Invoke(args);
				}

				return -1;
			}

			namedIndexCache.Add(nameKey, (name, index));

			return namedIndexes[name][index];
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			// Dispose managed state (managed objects)
			if (disposing)
			{
				parser.Dispose();
			}

			// Free unmanaged resources (unmanaged objects) and override finalizer
			// Set large fields to null
			context = null;

			disposed = true;
		}

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void CheckHasBeenRead()
		{
			if (!hasBeenRead)
			{
				throw new ReaderException(context, "You must call read on the reader before accessing its data.");
			}
		}

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected virtual void ParseNamedIndexes()
		{
			if (headerRecord == null)
			{
				throw new ReaderException(context, "No header record was found.");
			}

			namedIndexes.Clear();

			for (var i = 0; i < headerRecord.Length; i++)
			{
				var args = new PrepareHeaderForMatchArgs(headerRecord[i], i);
				var name = prepareHeaderForMatch(args);
				if (namedIndexes.ContainsKey(name))
				{
					namedIndexes[name].Add(i);
				}
				else
				{
					namedIndexes[name] = new List<int> { i };
				}
			}
		}
	}
}
