using System;
using System.Collections.Generic;

namespace LibraryManagement.Shared.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // "user" or "assistant"
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public BookEnrichment? BookData { get; set; }
}

public class BookEnrichment
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? CoverUrl { get; set; }
    public string? Description { get; set; }
    public string? Isbn { get; set; }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ChatHistoryItem> History { get; set; } = new();
}

public class ChatHistoryItem
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Response { get; set; } = string.Empty;
    public BookEnrichment? BookData { get; set; }
}
