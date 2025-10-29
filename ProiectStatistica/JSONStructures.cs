namespace ProiectStatistica;

public class Conversation {
	public Dictionary<string, MappingNode> Mapping { get; set; }
}

public class MappingNode {
	public string Id { get; set; }
	public MessageData Message { get; set; }
	public string Parent { get; set; }
	public List<string> Children { get; set; }
}

public class MessageData {
	public AuthorData Author { get; set; }
	public ContentData Content { get; set; }
}

public class AuthorData {
	public string Role { get; set; }
}

public class ContentData {
	public List<string> Parts { get; set; }
}