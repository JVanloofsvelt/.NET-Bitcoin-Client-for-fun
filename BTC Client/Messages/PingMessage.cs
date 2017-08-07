using System;

namespace BTCClient.Messages
{
    class PingMessage : Message
    {
        #region Static members
        public const string Command = "ping";

        public static PingMessage Parse(byte[] payload)
        {
            var reader = new PayloadReader(payload);
            var nonce = reader.ReadUInt64();

            reader.ThrowIfNotEndReached();

            return new PingMessage(nonce);
        }
        #endregion

        #region Instance members
        public UInt64 Nonce { get; private set; }

        public PingMessage(UInt64 nonce)
        {
            Nonce = nonce;
        }

        protected override string GetCommand()
        {
            return Command;
        }

        protected override byte[] GetPayload()
        {
            return Nonce.GetBytes();
        }
        #endregion
    }
}
