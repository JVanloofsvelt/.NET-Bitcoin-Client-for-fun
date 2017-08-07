using System;
using System.IO;
using System.Linq;
using System.Net;

namespace BTCClient.Messages
{
    class VersionMessage : Message
    {
        #region Static members
        public const string Command = "version";
        static readonly byte[] Services = { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };  // TODO: Fill in services

        public static VersionMessage Parse(byte[] payload)
        {
            var reader = new PayloadReader(payload);
            
            var protocolVersion = reader.ReadUInt32();
            reader.Seek(8, SeekOrigin.Current);
            var timestamp = reader.ReadDateTimeOffset(true);
            reader.Seek(8, SeekOrigin.Current);
            var to = reader.ReadIPEndPoint();
            reader.Seek(8, SeekOrigin.Current);
            var from = reader.ReadIPEndPoint();
            var nonce = reader.ReadUInt64();
            var userAgent = reader.ReadString();
            var startHeight = reader.ReadInt32();

            bool relay = true;

            if (reader.Position < payload.Length)
                relay = reader.ReadBool();

            if (reader.Position != payload.Length)
                throw new Exception($"Unparsed data remaining in {nameof(VersionMessage)}");

            return new VersionMessage(
                protocolVersion,
                timestamp,
                to,
                from,
                nonce,
                userAgent,
                startHeight,
                relay
            );
        }
        #endregion

        #region Instance members
        public UInt32 ProtocolVersion { get; private set; }
        public DateTimeOffset Timestamp { get; private set; }
        public IPEndPoint From { get; private set; }
        public IPEndPoint To { get; private set; }
        public UInt64 Nonce { get; private set; }
        public string UserAgent { get; private set; }
        public Int32 StartHeight { get; private set; }
        public bool Relay { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="to">IP Endpoint of node receiving this message.</param>
        /// <param name="from">IP Endpoint of node emitting this message.</param>
        /// <param name="nonce">Should be a random nonce used to detect connections to self.</param>
        /// <param name="startHeight">The last block received by the emitting node.</param>
        /// <param name="relay">Whether the remote peer should announce relayed transactions or not, see BIP 0037.</param>
        public VersionMessage(UInt32 protocolVersion, DateTimeOffset timestamp, IPEndPoint to, IPEndPoint from, UInt64 nonce, string userAgent, Int32 startHeight, bool relay)
        {
            ProtocolVersion = protocolVersion;
            Timestamp = timestamp;
            From = from;
            To = to;
            Nonce = nonce;
            UserAgent = userAgent;
            StartHeight = startHeight;
            Relay = relay;
        }

        protected override string GetCommand()
        {
            return Command;
        }

        protected override byte[] GetPayload()
        {
            return ProtocolVersion.GetBytes()
                .Concat(Services)
                .Concat(Timestamp.ToUnixTimeSeconds())
                .Concat(To)
                .Concat(From)
                .Concat(Nonce)
                .Concat(UserAgent)
                .Concat(StartHeight)
                .Concat(Relay)
                .ToArray();
        }
        #endregion
    }
}
