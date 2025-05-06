namespace ChatService.Domain.Models;

public class Group
{
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public List<string> AdminIds { get; set; } = new List<string>(); 
    public List<string> MemberIds { get; set; } = new List<string>(); 
}