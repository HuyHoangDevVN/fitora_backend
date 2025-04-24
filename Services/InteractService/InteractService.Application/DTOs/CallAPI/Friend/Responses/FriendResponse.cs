namespace InteractService.Application.DTOs.CallAPI.Friend.Responses;

public class FriendResponse
{
    public FriendData Data { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
}

public class FriendData
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int Count { get; set; }
    public List<Friend> Data { get; set; }
}

public class Friend
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public int Gender { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string Bio { get; set; }
}