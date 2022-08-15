using CsvHelper.Configuration;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CsvHelper;

/// <summary>
/// Used to write CSV files.
/// </summary>
public class CsvWriter : IDisposable
{
	private readonly TextWriter writer;
	private string newLine = "\r\n";
	private string quoteString = "\"";
	private string delimiter = ";";
	private readonly bool leaveOpen;
	private char quote = '"';
	private readonly char comment = '#';
	private readonly string escapeQuoteString;
	private readonly char[] injectionCharacters;
	private readonly char injectionEscapeCharacter;

	private int row = 1;
	private int index;
	private char[] buffer;
	private int bufferSize = 0x1000;
	private int bufferPosition;
	private bool disposed;

	/// <inheritdoc/>
	public int Index => index;

	/// <inheritdoc/>
	public virtual TrimOptions TrimOptions { get; set; }

	/// <inheritdoc/>
	public string[] HeaderRecord { get; private set; }


	/// <inheritdoc/>
	public int Row => row;

	/// <summary>
	/// Initializes a new instance of the <see cref="CsvWriter"/> class.
	/// </summary>
	/// <param name="writer">The writer.</param>
	public CsvWriter(TextWriter writer)
	{
		this.writer = writer;
		escapeQuoteString = new string(new[] { '"' });
		buffer = new char[bufferSize];
	}

	/// <summary>
	/// Writes the field to the CSV file. The field
	/// may get quotes added to it.
	/// When all fields are written for a record,
	/// <see cref="NextRecord()" /> must be called
	/// to complete writing of the current record.
	/// </summary>
	/// <param name="field">The field to write.</param>
	public void WriteField(string field)
	{
		if (field != null && (TrimOptions & TrimOptions.Trim) == TrimOptions.Trim)
		{
			field = field.Trim();
		}

		var shouldQuoteResult = ShouldQuote(field);

		WriteField(field, shouldQuoteResult);
	}

	/// <inheritdoc/>
	public virtual void WriteHeader(string[] headers)
	{
		var headerRecord = new List<string>();

		foreach (var header in headers)
		{
			WriteField(header);
			headerRecord.Add(header);
		}

		HeaderRecord = headerRecord.ToArray();
	}

	/// <summary>
	/// Writes a comment.
	/// </summary>
	/// <param name="text">The comment to write.</param>
	public void WriteComment(string text)
	{
		WriteField(comment + text, false);
	}


	/// <summary>
	/// Writes the field to the CSV file. This will
	/// ignore any need to quote and ignore 
	/// and just quote based on the shouldQuote
	/// parameter.
	/// When all fields are written for a record,
	/// <see cref="NextRecord()" /> must be called
	/// to complete writing of the current record.
	/// </summary>
	/// <param name="field">The field to write.</param>
	/// <param name="shouldQuote">True to quote the field, otherwise false.</param>
	public void WriteField(string field, bool shouldQuote)
	{


		// All quotes must be escaped.
		if (shouldQuote)
		{
			field = field?.Replace(quoteString, escapeQuoteString);
			field = quote + field + quote;
		}

		if (index > 0)
		{
			WriteToBuffer(delimiter);
		}

		WriteToBuffer(field);
		index++;
	}

	/// <summary>
	/// Returns true if the field contains a <see cref="IWriterConfiguration.Quote"/>,
	/// starts with a space, ends with a space, contains \r or \n, or contains
	/// the <see cref="IWriterConfiguration.Delimiter"/>.
	/// </summary>
	/// <param name="field">The column header.</param>
	/// <returns><c>true</c> if the field should be quoted, otherwise <c>false</c>.</returns>
	private bool ShouldQuote(string field)
	{
		var shouldQuote = !string.IsNullOrEmpty(field) &&
		(
			field.Contains(quote) // Contains quote
			|| field[0] == ' ' // Starts with a space
			|| field[field.Length - 1] == ' ' // Ends with a space
			|| (delimiter.Length > 0 && field.Contains(delimiter)) // Contains delimiter
			|| field.Contains(newLine) // Contains newline
		);

		return shouldQuote;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual string SanitizeForInjection(string field)
	{
		if (string.IsNullOrEmpty(field))
		{
			return field;
		}

		if (ArrayHelper.Contains(injectionCharacters, field[0]))
		{
			return injectionEscapeCharacter + field;
		}

		if (field[0] == quote && ArrayHelper.Contains(injectionCharacters, field[1]))
		{
			return field[0].ToString() + injectionEscapeCharacter.ToString() + field.Substring(1);
		}

		return field;
	}

	/// <inheritdoc/>
	public virtual void NextRecord()
	{
		WriteToBuffer(newLine);
		FlushBuffer();

		index = 0;
		row++;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void WriteToBuffer(string value)
	{
		var length = value?.Length ?? 0;

		if (value == null || length == 0)
		{
			return;
		}

		var lengthNeeded = bufferPosition + length;
		if (lengthNeeded >= bufferSize)
		{
			while (lengthNeeded >= bufferSize)
			{
				bufferSize *= 2;
			}

			Array.Resize(ref buffer, bufferSize);
		}

		value.CopyTo(0, buffer, bufferPosition, length);

		bufferPosition += length;
	}

	/// <inheritdoc/>
	public virtual async Task NextRecordAsync()
	{
		WriteToBuffer(newLine);
		await FlushBufferAsync();

		index = 0;
		row++;
	}

	/// <inheritdoc/>
	public virtual void Flush()
	{
		FlushBuffer();
		writer.Flush();
	}

	/// <inheritdoc/>
	public virtual async Task FlushAsync()
	{
		await FlushBufferAsync().ConfigureAwait(false);
		await writer.FlushAsync().ConfigureAwait(false);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual void FlushBuffer()
	{
		writer.Write(buffer, 0, bufferPosition);
		bufferPosition = 0;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual async Task FlushBufferAsync()
	{
		await writer.WriteAsync(buffer, 0, bufferPosition);
		bufferPosition = 0;
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

		Flush();

		if (disposing)
		{
			// Dispose managed state (managed objects)

			if (!leaveOpen)
			{
				writer.Dispose();
			}
		}

		// Free unmanaged resources (unmanaged objects) and override finalizer
		// Set large fields to null

		buffer = null;

		disposed = true;
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		await DisposeAsync(true).ConfigureAwait(false);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc/>
	protected virtual async ValueTask DisposeAsync(bool disposing)
	{
		if (disposed)
		{
			return;
		}

		await FlushAsync().ConfigureAwait(false);

		if (disposing)
		{
			// Dispose managed state (managed objects)

			if (!leaveOpen)
			{
				await writer.DisposeAsync().ConfigureAwait(false);
			}
		}

		// Free unmanaged resources (unmanaged objects) and override finalizer
		// Set large fields to null

		buffer = null;

		disposed = true;
	}
}
