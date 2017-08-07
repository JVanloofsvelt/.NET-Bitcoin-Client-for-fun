using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCClient.Messages
{
    abstract class Message
    {
        #region Static members
        static readonly byte[] Magic = new byte[] { 0xF9, 0xBE, 0xB4, 0xD9 };  // Main net, little endian
        const int MagicSize = 4;
        const int HeaderSize = 24;
        const int CommandSize = 12;
        const int CommandIndex = 4;
        const int PayloadLengthIndex = 16;
        const int ChecksumIndex = 20;
        
        public static async Task<Message> ReadMessageAsync(Stream stream)
        {
            var header = new byte[HeaderSize];
            int offset = 0, nRead;

            // Read leading magic bytes
            nRead = await stream.ReadAsync(header, offset, MagicSize).ConfigureAwait(false);

            if (nRead < Magic.Length)
                throw new EndOfStreamException();

            offset += nRead;


            // Check magic bytes
            if (!Magic.SequenceEqual(header.Take(MagicSize)))
                throw new Exception("Leading magic bytes are incorrect");


            // Read message type
            nRead = await stream.ReadAsync(header, offset, CommandSize).ConfigureAwait(false);
            offset += nRead;

            string command;

            try
            {
                command = System.Text.Encoding.ASCII.GetString(header, CommandIndex, nRead).TrimEnd('\0');
            }
            catch (DecoderFallbackException exception)
            {
                throw new Exception($"Failed to decode message type as ASCII string", exception);
            }

            if (nRead < CommandSize)
                throw new EndOfStreamException();


            // Read payload length
            nRead = await stream.ReadAsync(header, offset, sizeof(UInt32)).ConfigureAwait(false);
            var payloadLength = BitConverter.ToUInt32(header, PayloadLengthIndex);  // TODO: Take system's endianness in consideration

            if (nRead < sizeof(UInt32))
                throw new EndOfStreamException();

            offset += sizeof(UInt32);
            Debug.Assert(offset == HeaderSize - DoubleSHA256.ChecksumSize);


            // Read checksum
            nRead = await stream.ReadAsync(header, offset, DoubleSHA256.ChecksumSize).ConfigureAwait(false);

            if (nRead < DoubleSHA256.ChecksumSize)
                throw new EndOfStreamException();

            offset += DoubleSHA256.ChecksumSize;
            Debug.Assert(offset == HeaderSize);


            // Read payload
            var payload = new byte[payloadLength];

            nRead = await stream.ReadAsync(payload, 0, Convert.ToInt32(payloadLength)).ConfigureAwait(false);

            if (nRead < payloadLength)
                throw new EndOfStreamException();


            // Check checksum
            var checksum = DoubleSHA256.ComputeChecksum(payload);

            if (!header.Skip(ChecksumIndex).Take(DoubleSHA256.ChecksumSize).SequenceEqual(checksum))
                throw new Exception("Checksum error");


            // Parse
            return ParseMessage(command, payload);
        }

        private static Message ParseMessage(string command, byte[] payload)
        {
            try
            {
                if (command == AddrMessage.Command)
                    return AddrMessage.Parse(payload);

                if (command == InventoryMessage.Command)
                    return InventoryMessage.Parse(payload);

                if (command == PingMessage.Command)
                    return PingMessage.Parse(payload);

                if (command == VersionMessage.Command)
                    return VersionMessage.Parse(payload);

                if (command == VerAckMessage.Command)
                    return VerAckMessage.Parse(payload);

                if (command == AlertMessage.Command)
                    return AlertMessage.Parse(payload);
            }
            catch (Exception exception)
            {
                throw new Exception($"Failed to parse {command} message. See inner exception for details.", exception);
            }

            throw new Exception($"Received a message of unknown type: '{command}'");
        }


        #endregion

        #region Instance members
        protected abstract string GetCommand();

        protected abstract byte[] GetPayload();

        public byte[] Serialize()
        {
            byte[] command = null;

            try
            {
                command = System.Text.Encoding.ASCII.GetBytes(GetCommand());
            }
            catch (EncoderFallbackException)
            {
                // Probably contains non-ASCII characters
            }

            if (command == null || command.Length > CommandSize)
                throw new Exception($"{nameof(GetCommand)} should return an ASCII string containing a maximum of {CommandSize} characters");


            var payload = GetPayload() ?? new byte[] { };
            var checksum = DoubleSHA256.ComputeChecksum(payload);

            return Magic
                .Concat(command)
                .Concat(Enumerable.Repeat((byte)0, CommandSize - command.Length))
                .Concat(payload.Length)
                .Concat(checksum)
                .Concat(payload)
                .ToArray();
        }

        public override string ToString()
        {
            return GetType().Name;
        }
        #endregion
    }
}
