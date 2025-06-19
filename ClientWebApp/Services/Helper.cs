using System.IdentityModel.Tokens.Jwt;

namespace ClientWebApp.Services
{
    public class Helper
    {
        private Guid GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken == null) return Guid.Empty;

            string userIdClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "sub")?.Value;

            return Guid.TryParse(userIdClaim, out Guid userId) ? userId : Guid.Empty;
        }
    }
}
