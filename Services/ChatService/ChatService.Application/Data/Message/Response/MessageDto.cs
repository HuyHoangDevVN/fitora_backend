namespace ChatService.Application.Data.Message.Response;

public class MessageDto
{
    public string Id { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public string GroupId { get; set; }
    public string Content { get; set; }
    public string Type { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsRecalled { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsRead { get; set; }
    public List<Reaction> Reactions { get; set; } = new List<Reaction>();
}