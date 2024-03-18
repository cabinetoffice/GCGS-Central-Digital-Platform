using System.Text.Json;

namespace CO.CDP.OrganisationApp
{
    public class Session(IHttpContextAccessor httpContextAccessor) : ISession
    {
        public T? Get<T>(string key)
        {
            CheckSessionIsNull();

            var value = httpContextAccessor.HttpContext!.Session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public void Set<T>(string key, T value)
        {
            CheckSessionIsNull();

            httpContextAccessor.HttpContext!.Session
                .SetString(key, JsonSerializer.Serialize(value));
        }

        public void Remove(string key)
        {
            CheckSessionIsNull();

            httpContextAccessor.HttpContext!.Session.Remove(key);
        }

        private void CheckSessionIsNull()
        {
            try
            {
                if (httpContextAccessor.HttpContext?.Session == null)
                {
                    throw new Exception("Session is not available");
                }
            }
            catch (InvalidOperationException cause)
            {
                throw new Exception("Session is not available", cause);
            }
        }
    }
}
