using Microsoft.AspNetCore.Http;

namespace IIA.Helpers
{
    public static class UserHelper
    {
        public static bool IsLoggedIn(HttpContext context)
        {
            return !string.IsNullOrEmpty(context.Session.GetString("User"));
        }

        public static string GetUsername(HttpContext context)
        {
            return context.Session.GetString("User") ?? "Guest";
        }
    }
}
