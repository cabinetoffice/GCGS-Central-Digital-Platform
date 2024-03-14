using System.Text.Json;

namespace CO.CDP.OrganisationApp
{
    public class Session(IHttpContextAccessor httpContextAccessor) : ISession
    {
        public T? Get<T>(string key)
        {
            CheckSessionIsNull(httpContextAccessor.HttpContext);

            var value = httpContextAccessor.HttpContext!.Session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public void Set<T>(string key, T value)
        {
            CheckSessionIsNull(httpContextAccessor.HttpContext);

            httpContextAccessor.HttpContext!.Session
                .SetString(key, JsonSerializer.Serialize(value));
        }

        public void Remove(string key)
        {
            CheckSessionIsNull(httpContextAccessor.HttpContext);

            httpContextAccessor.HttpContext!.Session.Remove(key);
        }

        private static void CheckSessionIsNull(HttpContext? context)
        {
            if (context?.Session == null)
            {
                throw new Exception("Session is not available");
            }
        }
    }
}
