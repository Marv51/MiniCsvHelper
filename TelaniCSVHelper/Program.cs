// See https://aka.ms/new-console-template for more information
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

Console.WriteLine("Hello, World!");

using var stream = File.OpenRead(@"C:\Users\Arbeit\OneDrive - gb&t Gebäudebestand und Technik GmbH\Telani\Design Dokumente\Matrix Dokumente\Export-2021-11-24 17-52-43\aktorCSV-7igJ.csv");
string[]? header = null;
var encoding = Encoding.UTF8;
using var reader = new StreamReader(stream, encoding, true);
var guessedDelimiter = ";";

var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
{
	Delimiter = guessedDelimiter ?? ";",
	Encoding = encoding,
	BadDataFound = null
};

using var csv = new CsvReader(reader, csvConfiguration);

csv.Read();
csv.ReadHeader();
header = csv.Context.Reader.HeaderRecord;
for (var i = 0; i < header.Length; i++)
{
	header[i] = header[i].Trim();
	if (header[i].Length == 0)
	{
		header[i] = "Spalte " + (i + 1).ToString(CultureInfo.InvariantCulture);
	}
	else if (header.Contains(header[i]))
	{
		var index = 1;
		var regex = new System.Text.RegularExpressions.Regex("^" + header[i] + " (\\d+)$");

		var numbers = header.Where(s => regex.IsMatch(s)).Select(s => int.Parse(regex.Match(s).Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture));

		if (numbers.Any())
		{
			index = numbers.Max() + 1;
		}

		header[i] = header[i] + " " + index;
	}
}
Console.WriteLine(string.Join(", ", header));
while (csv.Read())
{
	string[] row = csv.Parser.Record;
	Console.WriteLine(string.Join(", ", row));
}
