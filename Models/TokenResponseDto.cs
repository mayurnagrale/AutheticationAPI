namespace AutheticationAPI.Models
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public Guid UserId { get; set; }
    }
}
