// File: TOTPUtility.cs

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace E2ETests.Utilities
{
    public static class TOTPUtility
    {
        public static string GenerateTOTP(string base32Secret)
        {
            byte[] secretBytes = Base32Decode(base32Secret);
            long timestep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;

            byte[] timestepBytes = new byte[8];
            long temp = timestep;
            for (int i = 7; i >= 0; i--)
            {
                timestepBytes[i] = (byte)(temp & 0xff);
                temp >>= 8;
            }

            using (var hmac = new HMACSHA1(secretBytes))
            {
                byte[] hash = hmac.ComputeHash(timestepBytes);
                int offset = hash[hash.Length - 1] & 0xf;
                int binary =
                    ((hash[offset] & 0x7f) << 24) |
                    ((hash[offset + 1] & 0xff) << 16) |
                    ((hash[offset + 2] & 0xff) << 8) |
                    (hash[offset + 3] & 0xff);
                int totp = binary % 1_000_000;
                return totp.ToString("D6"); // zero-pad to 6 digits
            }
        }

        private static byte[] Base32Decode(string base32)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            string clean = base32.Replace(" ", "").TrimEnd('=').ToUpperInvariant();

            List<byte> bytes = new List<byte>();
            int bitBuffer = 0;
            int bitsInBuffer = 0;

            foreach (char c in clean)
            {
                int val = alphabet.IndexOf(c);
                if (val < 0)
                    throw new ArgumentException($"Invalid Base32 character: {c}");

                bitBuffer = (bitBuffer << 5) | val;
                bitsInBuffer += 5;

                if (bitsInBuffer >= 8)
                {
                    bitsInBuffer -= 8;
                    bytes.Add((byte)(bitBuffer >> bitsInBuffer));
                    bitBuffer &= (1 << bitsInBuffer) - 1;
                }
            }

            return bytes.ToArray();
        }
    }
}
