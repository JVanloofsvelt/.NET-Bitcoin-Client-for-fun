using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace BTCClient.Messages
{
    static class Extensions
    {
        public static IEnumerable<byte> Concat(this IEnumerable<byte> sequence, bool value)
        {
            return sequence.Concat(value.GetBytes());
        }

        public static IEnumerable<byte> Concat(this IEnumerable<byte> sequence, string value)
        {
            return sequence.Concat(value.GetBytes());
        }

        public static IEnumerable<byte> Concat(this IEnumerable<byte> sequence, IPEndPoint value)
        {
            return sequence.Concat(value.GetBytes());
        }

        public static IEnumerable<byte> Concat(this IEnumerable<byte> sequence, UInt64 value)
        {
            return sequence.Concat(value.GetBytes());
        }

        public static IEnumerable<byte> Concat(this IEnumerable<byte> sequence, Int64 value)
        {
            return sequence.Concat(value.GetBytes());
        }

        public static IEnumerable<byte> Concat(this IEnumerable<byte> sequence, UInt32 value)
        {
            return sequence.Concat(value.GetBytes());
        }

        public static IEnumerable<byte> Concat(this IEnumerable<byte> sequence, Int32 value)
        {
            return sequence.Concat(value.GetBytes());
        }

        public static byte[] GetBytes(this bool value)
        {
            return new [] { (byte)(value ? 1 : 0) };
        }

        public static byte[] GetBytes(this string value)
        {
            var length = Convert.ToUInt32(value?.Length ?? 0);

            return GetVarIntBytes(length).Concat(System.Text.Encoding.ASCII.GetBytes(value)).ToArray();
        }

        public static byte[] GetVarIntBytes(this ulong value)
        {
            if (value < 0xFD)
            {
                return new byte[] { Convert.ToByte(value) };
            }

            if (value < 0xFFFF)
            {
                return new byte[]{ 0xFD }.Concat(BitConverter.GetBytes(Convert.ToUInt16(value))).ToArray();
            }

            if (value < 0xFFFFFFFF)
            {
                return new byte[] { 0xFE }.Concat(BitConverter.GetBytes(Convert.ToUInt32(value))).ToArray();
            }

            return new byte[] { 0xFF }.Concat(BitConverter.GetBytes(Convert.ToUInt64(value))).ToArray();
        }

        public static byte[] GetBytes(this IPEndPoint value, DateTimeOffset? timestamp=null)
        {
            if (timestamp != null)
                throw new NotImplementedException();

            var address = value.Address.GetBytes();
            var port = Convert.ToUInt16(value.Port).GetBytes();

            return new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }.Concat(address).Concat(port).ToArray();  // TODO: Fill in services
        }

        public static byte[] GetBytes(this IPAddress value)
        {
            return value.MapToIPv6().GetAddressBytes();
        }

        public static byte[] GetBytes(this UInt64 value)
        {
            return BitConverter.GetBytes(value).FixEndianness();
        }

        public static byte[] GetBytes(this Int64 value)
        {
            return BitConverter.GetBytes(value).FixEndianness();
        }

        public static byte[] GetBytes(this UInt32 value)
        {
            return BitConverter.GetBytes(value).FixEndianness();
        }

        public static byte[] GetBytes(this Int32 value)
        {
            return BitConverter.GetBytes(value).FixEndianness();
        }

        public static byte[] GetBytes(this UInt16 value)
        {
            return BitConverter.GetBytes(value).FixEndianness();
        }

        private static byte[] FixEndianness(this byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
                return bytes.Reverse().ToArray();
            else
                return bytes;
        }
    }
}
