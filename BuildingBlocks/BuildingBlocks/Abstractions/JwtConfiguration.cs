namespace BuildingBlocks.Abstractions;

#nullable disable
public class JwtConfiguration
{
    public string Secret { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
}