namespace CreativityUI.Features.Auth.Services;

public interface ITokenValidationService
{
    bool IsTokenValid(string? token);
}
