﻿namespace AuthService.Application.Auths.Queries.GetUser;

public record GetUserQuery(string UserId) : IQuery<GetUserResult>;

public record GetUserResult(
    string UserId,
    string Email,
    string Username,
    string FullName,
    string PhoneNumber,
    string Avatar,
    int Status = 0,
    IList<string> Roles = null!);