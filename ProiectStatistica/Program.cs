using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ProiectStatistica;


Console.ForegroundColor = ConsoleColor.White;

Console.WriteLine("Enter path to conversations json...");
string pathToJSON = Console.ReadLine() ?? throw new InvalidOperationException();

var jsonText =  File.ReadAllText(pathToJSON);
var options = new JsonSerializerOptions {
	PropertyNameCaseInsensitive = true
};

var conversations = JsonSerializer.Deserialize<List<Conversation>>(jsonText, options);

foreach (var conversation in conversations) {
	if (conversation?.Mapping == null) continue;

	var currentNodeId = conversation.Mapping.FirstOrDefault(x => x.Value.Parent == null).Value.Children.FirstOrDefault();
	while (!string.IsNullOrEmpty(currentNodeId) && conversation.Mapping.ContainsKey(currentNodeId)) {
		var currentNode = conversation.Mapping[currentNodeId];
		if (currentNode.Message?.Author?.Role != null && currentNode.Message.Content?.Parts != null) {
			var role = currentNode.Message.Author.Role;
			var content = string.Join(" ", currentNode.Message.Content.Parts);

			if (!string.IsNullOrWhiteSpace(content)) Console.WriteLine($"{role}:\n{content}\nFILTERED:\n{FilterToWords(content)}");
		}

		currentNodeId = currentNode.Children.FirstOrDefault();
	}
}
	
	
	
	
Console.ReadLine();
return;




string[] FilterToWords(string input) {
	if (string.IsNullOrWhiteSpace(input)) return [];

	var processedString = input.ToLower();
	processedString = Regex.Replace(processedString, @"[^\p{L}\s]", " ");
	processedString = Regex.Replace(processedString, @"\s+", " ");

	var words = processedString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
	
	return words;
}


void PrintColored(string message, ConsoleColor fgColor) {
	Console.ForegroundColor =  fgColor;
	Console.WriteLine(message);
	Console.ForegroundColor = ConsoleColor.White;
}