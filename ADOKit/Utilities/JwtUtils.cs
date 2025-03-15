using System;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;


namespace ADOKit.Utilities
{
    class JwtUtils
    {
        // https://stackoverflow.com/a/74698169
        // Check for Expiration & Audience
        public static bool isValid(string token)
        {  
            JwtSecurityToken jwtSecurityToken;
            try
            {
                jwtSecurityToken = new JwtSecurityToken(token);
            }
            catch (Exception)
            {
                Console.WriteLine("[-] ERROR: The provided token is not a valid JWT. PATs not supported for this module.");
                return false;
            }

            bool notExpired = jwtSecurityToken.ValidTo > DateTime.UtcNow;
            bool hasValidAudience = jwtSecurityToken.Audiences.Contains("499b84ac-1321-427f-aa17-267ca6975798") || jwtSecurityToken.Audiences.Contains("https://management.core.windows.net/");

            if(!notExpired)
            {
                Console.WriteLine("[-] ERROR: JWT Token Expired!");
            }
            if (!hasValidAudience)
            {
                Console.WriteLine("[-] ERROR: JWT Invalid Audience!");
            }

            return notExpired && hasValidAudience;
        }

        public static string getTenantID(string token)
        {
            JwtSecurityToken jwtSecurityToken;
            try
            {
                jwtSecurityToken = new JwtSecurityToken(token);
            }
            catch (Exception)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: The provided token is not a valid JWT. PATs not supported.");
                Console.WriteLine("");
                return "";
            }

            // Extract the "tid" claim value
            var tenantId = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;

            return tenantId ?? "";
        }
    }
}
