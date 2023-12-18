// Copyright 2009-2021 Josh Close
// This file is a part of CsvHelper and is dual licensed under MS-PL and Apache 2.0.
// See LICENSE.txt for details or visit http://www.opensource.org/licenses/ms-pl.html for MS-PL and http://opensource.org/licenses/Apache-2.0 for Apache 2.0.
// https://github.com/JoshClose/CsvHelper

namespace CsvHelper.Configuration;

/// <summary>Holds the default callback methods for delegate members of <c>CsvHelper.Configuration.Configuration</c>.</summary>
public static class ConfigurationFunctions
{
	private static readonly char[] lineEndingChars = ['\r', '\n'];

	/// <summary>
	/// Throws a <c>MissingFieldException</c>.
	/// </summary>
	public static void MissingFieldFound(MissingFieldFoundArgs args)
	{
		var messagePostfix = $"You can ignore missing fields by setting {nameof(MissingFieldFound)} to null.";

		// Get by index.

		if (args.HeaderNames == null || args.HeaderNames.Length == 0)
		{
			throw new MissingFieldException(args.Context, $"Field at index '{args.Index}' does not exist. {messagePostfix}");
		}

		// Get by name.

		var indexText = args.Index > 0 ? $" at field index '{args.Index}'" : string.Empty;

		if (args.HeaderNames.Length == 1)
		{
			throw new MissingFieldException(args.Context, $"Field with name '{args.HeaderNames[0]}'{indexText} does not exist. {messagePostfix}");
		}

		throw new MissingFieldException(args.Context, $"Field containing names '{string.Join("' or '", args.HeaderNames)}'{indexText} does not exist. {messagePostfix}");
	}

	/// <summary>
	/// Throws a <see cref="BadDataException"/>.
	/// </summary>
	public static void BadDataFound(BadDataFoundArgs args)
	{
		throw new BadDataException(args.Context, $"You can ignore bad data by setting {nameof(BadDataFound)} to null.");
	}

	/// <summary>
	/// Throws the given <see name="ReadingExceptionOccurredArgs.Exception"/>.
	/// </summary>
	public static bool ReadingExceptionOccurred(ReadingExceptionOccurredArgs args)
	{
		return true;
	}

	/// <summary>
	/// Returns <c>false</c>.
	/// </summary>
	public static bool ShouldSkipRecord(ShouldSkipRecordArgs args)
	{
		return false;
	}

	/// <summary>
	/// Returns the <see name="PrepareHeaderForMatchArgs.Header"/> as given.
	/// </summary>
	public static string PrepareHeaderForMatch(PrepareHeaderForMatchArgs args)
	{
		return args.Header;
	}
}
