using System;
using System.Collections.Concurrent;

namespace E2ETests.Utilities
{
    public static class StorageUtility
    {
        private static readonly ConcurrentDictionary<string, string> _storage = new ConcurrentDictionary<string, string>();

        /// Stores an organisation ID using a custom key.
        public static void Store(string key, string value)
        {
            _storage[key] = value;
            Console.WriteLine($"📌 Stored: {key} => {value}");
        }

        /// Retrieves an organisation ID by key.
        /// Throws an exception if the key does not exist.
        public static string Retrieve(string key)
        {
            if (_storage.TryGetValue(key, out var value))
            {
                return value;
            }
            throw new Exception($"❌ Key '{key}' not found in StorageUtility.");
        }

        /// Checks if a key exists in storage.
        public static bool Exists(string key)
        {
            return _storage.ContainsKey(key);
        }
    }
}
