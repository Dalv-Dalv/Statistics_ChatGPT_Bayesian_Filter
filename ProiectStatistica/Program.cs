using System.Text.RegularExpressions;






var data = ReadCSV(@"C:\Dalv\School\University\Classes\Semestrul3\PS\Proiect\toxicity.csv");

int totalMessagesCount = data.Length;
int toxicMessagesCount = 0;

Dictionary<string, int> toxicWordAppearances = new();
Dictionary<string, int> neutralWordAppearances = new();
Dictionary<string, int> wordAppearances = new();

foreach (var (msg, isToxic) in data) {
	if (isToxic) toxicMessagesCount++;

	var words = msg.Split(" ");
	foreach (var word in words) {
		if(!wordAppearances.TryAdd(word, 1)) wordAppearances[word]++;
		
		if (isToxic) {
			if(!toxicWordAppearances.TryAdd(word, 1)) toxicWordAppearances[word]++;
		} else {
			if(!neutralWordAppearances.TryAdd(word, 1)) neutralWordAppearances[word]++;
		}
	}
}

var totalToxicWords = toxicWordAppearances.Count;
var totalNeutralWords = neutralWordAppearances.Count;
var vocabularySize = wordAppearances.Count;

double pToxic = (double)toxicMessagesCount / totalMessagesCount;
double pNeutral = 1.0d - pToxic;
double alpha = 1.0;


string? message;
while (!string.IsNullOrEmpty(message = Console.ReadLine())) {
	Console.WriteLine($"\nYour message is {(IsMessageToxic(message) ? "TOXIC" : "neutral")}");
}








return;
string CleanMessage(string input)  {
	if (string.IsNullOrWhiteSpace(input))
		return string.Empty;

	input = input.ToLowerInvariant();
	input = Regex.Replace(input, @"[^a-z0-9?!\s]", "");
	input = Regex.Replace(input, @"\s+", " ");
	input = Regex.Replace(input, @"[!?]{2,}", match => {
		var s = match.Value;
		if (s.Contains('?') && s.Contains('!'))
			return "?!";
		if (s.Contains('?'))
			return "?";
		return "!";
	});
	input = input.Trim();

	return input;
}


(string, bool)[] ReadCSV(string path) {
	string[] lines = File.ReadAllLines(path);
	
	var data = new (string, bool)[lines.Length];

	for (int i = 0; i < lines.Length; i++) {
		var msg = lines[i];
		msg = msg[(msg.LastIndexOf(',',  msg.Length - 4) + 1)..^2];
		msg = CleanMessage(msg);
		
		bool isToxic = lines[i][^1] != '0';

		data[i] = (msg, isToxic);
	}

	return data;
}


double CalculateWordProbability(string word, bool isToxic) { // P(word|toxic)
	if (isToxic) {
		toxicWordAppearances.TryGetValue(word, out var wordCount);
		return (wordCount + alpha) / (totalToxicWords + alpha * vocabularySize);
	} else {
		neutralWordAppearances.TryGetValue(word, out var wordCount);
		return (wordCount + alpha) / (totalNeutralWords + alpha * vocabularySize);
	}
}


bool IsMessageToxic(string message) {
	string[] words = message.Split().Select(CleanMessage).ToArray();

	double logScoreToxic = Math.Log(pToxic);
	double logScoreNeutral = Math.Log(pNeutral);

	foreach (var word in words) {
		logScoreToxic += Math.Log(CalculateWordProbability(word, isToxic: true));
		logScoreNeutral += Math.Log(CalculateWordProbability(word, isToxic: false));
	}

	return logScoreToxic > logScoreNeutral;
}