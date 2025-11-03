using System.Text.RegularExpressions;

Console.WriteLine("Reading and processing data...");
Console.ForegroundColor = ConsoleColor.DarkGray;

var data = ReadCSV(@"C:\Dalv\School\University\Classes\Semestrul3\PS\Proiect\toxicity.csv");

var totalMessagesCount = data.Length;
var toxicMessagesCount = 0;

Dictionary<string, int> toxicWordAppearances = new();
Dictionary<string, int> neutralWordAppearances = new();
Dictionary<string, int> wordAppearances = new();

HashSet<string> ignoredWords = ["you", "u", "the", "are", "i", "to", "in", "for", "an", "so"];


// Populate dictionaries
foreach (var (msg, isToxic) in data) {
	if (isToxic) toxicMessagesCount++;

	var words = msg.Split(" ");

	for (var i = 0; i < words.Length; i++) {
		var word = words[i];
		if (!ignoredWords.Contains(word)) AddToken(word, isToxic);

		if (i >= words.Length - 1) continue;

		var compoundWord = words[i] + " " + words[i + 1];
		if (!ignoredWords.Contains(compoundWord)) AddToken(compoundWord, isToxic);
	}
}

var totalToxicWords = toxicWordAppearances.Values.Sum();
var totalNeutralWords = neutralWordAppearances.Values.Sum();
var vocabularySize = wordAppearances.Count;
var pToxic = (double)toxicMessagesCount / totalMessagesCount;
var pNeutral = 1.0d - pToxic;

const double alpha = 1.0;
const double uselessnessThreshold = 0.0005d / 100d;

// Filter out words that have a very close probability of being toxic as they are of being neutral
foreach (var (msg, _) in data) {
	var words = msg.Split(" ");

	for (var i = 0; i < words.Length; i++) {
		var word = words[i];

		var diff = CalculateWordProbability(word, true) - CalculateWordProbability(word, false);
		diff = Math.Abs(diff);

		if (diff <= uselessnessThreshold) {
			Console.WriteLine($"Found \"useless\" word: {word}    (P({word}|toxic)={CalculateWordProbability(word, true) * 100:0.0000}%, P({word}|neutral)={CalculateWordProbability(word, false) * 100:0.0000}%");
			ignoredWords.Add(word);
		}

		if (i >= words.Length - 1) continue;

		var compoundWord = words[i] + " " + words[i + 1];
		diff = CalculateWordProbability(compoundWord, true) - CalculateWordProbability(compoundWord, false);
		diff = Math.Abs(diff);

		if (diff <= uselessnessThreshold) {
			Console.WriteLine($"Found \"useless\" word: {compoundWord}    (P({compoundWord}|toxic)={CalculateWordProbability(compoundWord, true) * 100:0.0000}%, P({compoundWord}|neutral)={CalculateWordProbability(compoundWord, false) * 100:0.0000}%");
			ignoredWords.Add(compoundWord);
		}
	}
}


Console.ForegroundColor = ConsoleColor.Gray;
Console.WriteLine("Finished, you can now input messages:");


string? message;
while (!string.IsNullOrEmpty(message = Console.ReadLine())) Console.WriteLine($"Your message is {(IsMessageToxic(message) ? "TOXIC" : "neutral")}\n");


return;

string CleanMessage(string input) {
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
	var lines = File.ReadAllLines(path);

	var data = new (string, bool)[lines.Length];

	for (var i = 0; i < lines.Length; i++) {
		var msg = lines[i];
		msg = msg[(msg.LastIndexOf(',', msg.Length - 4) + 1)..^2];
		msg = CleanMessage(msg);

		var isToxic = lines[i][^1] != '0';

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
	var words = CleanMessage(message).Split();

	var logScoreToxic = Math.Log(pToxic);
	var logScoreNeutral = Math.Log(pNeutral);

	Console.ForegroundColor = ConsoleColor.DarkGray;

	Console.WriteLine($"P(toxic) = {pToxic * 100:0.00}%");
	Console.WriteLine($"P(neutral) = {pNeutral * 100:0.00}%");

	Console.WriteLine($"Current scores: Toxicity {logScoreToxic:0.00}   Neutral {logScoreNeutral:0.00}");


	for (var i = 0; i < words.Length; i++) {
		var word = words[i];

		if (!ignoredWords.Contains(word)) {
			logScoreToxic += Math.Log(CalculateWordProbability(word, true));
			logScoreNeutral += Math.Log(CalculateWordProbability(word, false));

			Console.WriteLine($"P({word}|toxic) = {CalculateWordProbability(word, true) * 100:0.00}%");
			Console.WriteLine($"P({word}|neutral) = {CalculateWordProbability(word, false) * 100:0.00}%");
			Console.WriteLine($"Current scores: Toxicity {logScoreToxic:0.00}   Neutral {logScoreNeutral:0.00}");
		}

		if (i >= words.Length - 1) continue;

		var compoundWord = words[i] + " " + words[i + 1];
		logScoreToxic += Math.Log(CalculateWordProbability(compoundWord, true));
		logScoreNeutral += Math.Log(CalculateWordProbability(compoundWord, false));

		Console.WriteLine($"P({compoundWord}|toxic) = {CalculateWordProbability(compoundWord, true) * 100:0.00}%");
		Console.WriteLine($"P({compoundWord}|neutral) = {CalculateWordProbability(compoundWord, false) * 100:0.00}%");
		Console.WriteLine($"Current scores: Toxicity {logScoreToxic:0.00}   Neutral {logScoreNeutral:0.00}");
	}

	Console.WriteLine($"Toxic score: {logScoreToxic:0.00}  Neutral Score: {logScoreNeutral:0.00}");


	Console.ForegroundColor = ConsoleColor.Gray;


	return logScoreToxic > logScoreNeutral;
}

void AddToken(string word, bool isToxic) {
	if (!wordAppearances.TryAdd(word, 1)) wordAppearances[word]++;

	if (isToxic) {
		if (!toxicWordAppearances.TryAdd(word, 1)) toxicWordAppearances[word]++;
	} else {
		if (!neutralWordAppearances.TryAdd(word, 1)) neutralWordAppearances[word]++;
	}
}