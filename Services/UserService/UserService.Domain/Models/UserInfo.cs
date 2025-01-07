using System.Text.Json.Serialization;
using BuildingBlocks.Abstractions;
using UserService.Domain.Enums;

namespace UserService.Domain.Models;

public class UserInfo : Entity<Guid>
{
    public Guid UserId { get; set; }
    [JsonIgnore] 
    public User User { get; set; } = default!;
    public string? FirstName { get; set; } = String.Empty;
    public string? LastName { get; set; } = String.Empty;
    public DateTime? BirthDate { get; set; } = DateTime.Today;
    public Gender Gender { get; set; } = Gender.Unknown;
    public string? Address { get; set; } = String.Empty;
    public string? PhoneNumber { get; set; } = String.Empty;
    public string? ProfilePictureUrl { get; set; } = String.Empty;
    public string? Bio { get; set; } = String.Empty;
}