namespace CsvHelper.Tests;

public class XunitException : Exception
{
	public XunitException() : base() { }

	public XunitException(string message) : base(message) { }
    }
