using System.Text;
using System.Text.Json;

namespace CreativityUI.Features.Auth.Services;

public sealed class JwtTokenValidationService : ITokenValidationService
{
    public bool IsTokenValid(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var parts = token.Split('.');
        if (parts.Length < 2)
        {
            return false;
        }

        try
        {
            var payloadBytes = DecodeBase64Url(parts[1]);
            using var document = JsonDocument.Parse(payloadBytes);

            if (!document.RootElement.TryGetProperty("exp", out var expElement))
            {
                return false;
            }

            long expUnixSeconds = expElement.ValueKind switch
            {
                JsonValueKind.Number when expElement.TryGetInt64(out var numberValue) => numberValue,
                JsonValueKind.String when long.TryParse(expElement.GetString(), out var stringValue) => stringValue,
                _ => 0
            };

            if (expUnixSeconds <= 0)
            {
                return false;
            }

            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnixSeconds);
            return expiresAt > DateTimeOffset.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    private static byte[] DecodeBase64Url(string input)
    {
        var base64 = input.Replace('-', '+').Replace('_', '/');
        var padding = 4 - (base64.Length % 4);
        if (padding is > 0 and < 4)
        {
            base64 = base64.PadRight(base64.Length + padding, '=');
        }

        return Convert.FromBase64String(base64);
    }
}
