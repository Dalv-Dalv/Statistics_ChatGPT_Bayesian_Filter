using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

public static class ToxicCommentsParser {
	public static (string message, string typeOfToxicity)[] ParseCsv(string path) {
		var results = new List<(string, string)>();

		var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
			HasHeaderRecord = true,
			IgnoreBlankLines = true,
			BadDataFound = null,
			DetectDelimiter = true,
			Quote = '"',
			MissingFieldFound = null,
			TrimOptions = TrimOptions.Trim
		};

		using (var reader = new StreamReader(path))
		using (var csv = new CsvReader(reader, config)) {
			while (csv.Read())
				try {
					string comment = csv.GetField("comment_text");
					int toxic = csv.GetField<int>("toxic");
					int severe = csv.GetField<int>("severe_toxic");
					int obscene = csv.GetField<int>("obscene");
					int threat = csv.GetField<int>("threat");
					int insult = csv.GetField<int>("insult");
					int identityHate = csv.GetField<int>("identity_hate");

					var type =
						toxic == 1 ? "toxic" :
						severe == 1 ? "severe_toxic" :
						obscene == 1 ? "obscene" :
						threat == 1 ? "threat" :
						insult == 1 ? "insult" :
						identityHate == 1 ? "identity_hate" :
						"none";

					results.Add((comment, type));
				} catch { /* skip bad rows */
				}
		}

		return results.ToArray();
	}
}