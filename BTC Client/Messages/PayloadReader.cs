using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace BTCClient.Messages
{
    class PayloadReader
    {
        const int IPv6AddressSize = 16;

        byte[] payload;

        public int Position { get; private set; } = 0;

        public PayloadReader(byte[] payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            this.payload = payload;
        }

        public void ThrowIfNotEndReached()
        {
            if (!EndOfPayload)
                throw new Exception($"There are unread bytes in the payload");
        }

        public bool EndOfPayload
        {
            get { return Position == payload.Length; }
        }

        public void Seek(int offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = payload.Length - offset;
                    break;

                default:
                    throw new ArgumentException("Parameter value not supported", nameof(origin));
            }
        }

        public InventoryVector ReadInventoryVector()
        {
            var type = (ObjectType)ReadUInt32();
            var hash = ReadArray(DoubleSHA256.HashSize);

            return new InventoryVector(type, hash);
        }

        /// <summary>
        /// </summary>
        /// <param name="longer">Parses an UInt32 if false, an Int64 if true.</param>
        /// <returns></returns>
        public DateTimeOffset ReadDateTimeOffset(bool longer)
        {
            Int64 timestamp;

            if (longer)
                timestamp = ReadInt64();
            else
                timestamp = Convert.ToInt64(ReadUInt32());

            return DateTimeOffset.FromUnixTimeSeconds(timestamp);
        }

        public bool ReadBool()
        {
            bool result;

            switch (payload[Position])
            {
                case 0:
                    result = false;
                    break;

                case 1:
                    result = true;
                    break;

                default:
                    throw new Exception("Byte to be interpreted as a bool has an invalid value: it should be either 1 or 0");
            }

            Position += 1;
            return result;
        }

        public void SkipString()
        {
            var length = Convert.ToInt32(ReadVarInt());
            Position += Convert.ToInt32(length);
        }

        public string ReadString()
        {
            var length = Convert.ToInt32(ReadVarInt());
            var result = System.Text.Encoding.ASCII.GetString(payload, Position, length);
            Position += length;
            return result;
        }

        public UInt64 ReadVarInt()
        {
            var first = payload[Position];
            Position += 1;

            UInt64 result;

            switch (first)
            {
                case 0xFD:
                    result = ReadUInt16();
                    break;

                case 0xFE:
                    result = ReadUInt32();
                    break;

                case 0xFF:
                    result = ReadUInt64();
                    break;

                default:
                    result = first;
                    break;
            }

            return result;
        }

        public IPEndPoint ReadIPEndPoint()
        {
            var address = ReadIPAddress();
            var port = ReadUInt16(networkOrder: true);

            if (address.IsIPv4MappedToIPv6)
                address = address.MapToIPv4();

            return new IPEndPoint(address, port);
        }

        public IPAddress ReadIPAddress()
        {
            if (payload.Length < Position + IPv6AddressSize)
                throw new Exception("Not enough bytes left to read an IP address");

            var address = new IPAddress(payload.Skip(Position).Take(IPv6AddressSize).ToArray());
            Position += IPv6AddressSize;

            return address;
        }

        public byte[] ReadArray(int length)
        {
            var result = payload.Skip(Position).Take(length).ToArray();
            Position += length;
            return result;
        }

        public UInt16 ReadUInt16(bool networkOrder=false)
        {
            UInt16 result;

            if (networkOrder)
            {
                var bytes = payload.Skip(Position).Take(sizeof(UInt16)).Reverse().ToArray();
                result = BitConverter.ToUInt16(bytes, 0);
            }
            else
                result = BitConverter.ToUInt16(payload, Position);

            Position += sizeof(UInt16);
            return result;
        }

        public Int32 ReadInt32()
        {
            var result = BitConverter.ToInt32(payload, Position);
            Position += sizeof(Int32);
            return result;
        }

        public UInt32 ReadUInt32()
        {
            var result = BitConverter.ToUInt32(payload, Position);
            Position += sizeof(UInt32);
            return result;
        }

        public Int64 ReadInt64()
        {
            var result = BitConverter.ToInt64(payload, Position);
            Position += sizeof(Int64);
            return result;
        }

        public UInt64 ReadUInt64()
        {
            var result = BitConverter.ToUInt64(payload, Position);
            Position += sizeof(UInt64);
            return result;
        }
    }
}
