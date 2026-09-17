using AuthService.Application.DTOs.Key;

namespace AuthService.Application.Extensions;

public static class KeyExtensions
{
    public static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return "***";
        if (token.Length <= 8) return token[..Math.Min(2, token.Length)] + "***";
        return token[..4] + "***" + token[^4..];
    }

    public static IEnumerable<KeyDto> KeyToDto(IEnumerable<Key> keys)
    {
        var keyDtos = keys.Select(x => new KeyDto(
            x.Id.Value,
            x.UserId,
            MaskToken(x.Token),
            x.CreatedAt,
            x.Expires,
            x.IsUsed,
            x.IsRevoked));

        return keyDtos;
    }
}


