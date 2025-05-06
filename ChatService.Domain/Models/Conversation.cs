namespace ChatService.Domain.Models;

public class Conversation
{
    public string Id { get; set; }
    public List<string> ParticipantIds { get; set; } = new List<string>(); 
    public DateTime CreatedAt { get; set; }
    public bool IsGroup { get; set; } 
    public Group GroupInfo { get; set; }
}