using System.Security.Claims;

namespace KeyVault.Services
{
    public static class HelpUtils
    {
        public static bool IsValidClient(string valueToCheck, ClaimsPrincipal User)
        {
            var dataKey = valueToCheck;

            if (!dataKey.Contains("::"))
            {
                return false;
            }

            if (dataKey.Split(':')[0] != User.FindFirst(ClaimTypes.Thumbprint).Value)
            {
                return false;
            }

            return true;
        }
    }
}
