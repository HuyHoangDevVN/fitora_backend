﻿namespace AuthService.Application.DTOs.Users.Responses;

public record GetUserResponseDto(string UserId,
    string Email,
    string Username,
    string FullName,
    string PhoneNumber,
    string Avatar,
    int Status = 0,
    IList<string> Roles = null!);