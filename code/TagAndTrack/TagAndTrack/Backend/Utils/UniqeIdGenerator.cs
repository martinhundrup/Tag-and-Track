using System;
using System.Security.Cryptography;

namespace TagAndTrack.Backend
{
    internal static class UniqueIdGenerator
    {
        private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        /// <summary>
        /// Generates a 64-bit unsigned random ID.
        /// Collision probability is effectively zero for any realistic number of items.
        /// </summary>
        public static ulong NewId()
        {
            Span<byte> bytes = stackalloc byte[8];
            Rng.GetBytes(bytes);
            return BitConverter.ToUInt64(bytes);
        }

        /// <summary>
        /// Optional: generate a shorter string ID (base 36) if you do not want long numbers.
        /// </summary>
        public static string NewStringId()
        {
            var id = NewId();
            return ToBase36(id);
        }

        private static string ToBase36(ulong value)
        {
            const string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Span<char> buffer = stackalloc char[13]; // enough for 64-bit in base 36
            int pos = buffer.Length;

            do
            {
                buffer[--pos] = alphabet[(int)(value % 36)];
                value /= 36;
            } while (value > 0);

            return new string(buffer[pos..]);
        }
    }
}
