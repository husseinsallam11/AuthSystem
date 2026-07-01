namespace Auth.Application.DTOs
{
    public class RefreshTokenDto
    {
        public string token { get; set; } = string.Empty;
        public string refreshToken { get; set; } = string.Empty;
    }
}
