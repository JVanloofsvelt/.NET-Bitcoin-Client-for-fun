using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;

namespace BTCClient.Messages
{
    class AddrMessage : Message
    {
        #region Static members
        public const string Command = "addr";

        public static AddrMessage Parse(byte[] payload)
        {
            var reader = new PayloadReader(payload);

            var count = reader.ReadVarInt();  // TODO: Warn when count exceeds 1000, this ought to be a maximum
            var nodeInfos = new NodeInfo[count];  // TODO: Don't allow peer to force us to allocate a lot of memory here

            for (UInt64 i = 0; i < count; i++)
            {
                var timestamp = reader.ReadDateTimeOffset(longer: false); // Clients with protocol version >= 31402 will provide this timestamp
                reader.Seek(8, SeekOrigin.Current);
                var endpoint = reader.ReadIPEndPoint();

                nodeInfos[i] = new NodeInfo(timestamp, endpoint);
            }

            reader.ThrowIfNotEndReached();

            return new AddrMessage
            {
                NodeInfos = nodeInfos
            };
        }
        #endregion

        #region Instance members
        public NodeInfo[] NodeInfos { get; private set; }

        protected override string GetCommand()
        {
            return Command;
        }

        protected override byte[] GetPayload()
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    class NodeInfo
    {
        public DateTimeOffset Timestamp { get; private set; }
        public IPEndPoint EndPoint { get; private set; }

        //public byte[] Services { get; private set; }  // TODO: put this to use

        public NodeInfo(DateTimeOffset timestamp, IPEndPoint endpoint)
        {
            Timestamp = timestamp;
            EndPoint = endpoint;
        }
    }
}
