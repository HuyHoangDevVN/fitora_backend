﻿using AuthService.Application.DTOs.Key;

namespace AuthService.Application.Extensions;

public static class KeyExtensions
{

    public static IEnumerable<KeyDto> KeyToDto(IEnumerable<Key> keys)
    {
        var keyDtos = keys.Select(x => new KeyDto(x.UserId, x.Token, x.Expires, x.IsUsed, x.IsRevoked));

        return keyDtos;
    }
}


