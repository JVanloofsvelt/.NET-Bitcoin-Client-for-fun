using System;

namespace BTCClient.Messages
{
    class PongMessage : Message
    {
        #region Static members
        public const string Command = "pong";

        public static PongMessage Parse(byte[] payload)
        {
            var reader = new PayloadReader(payload);
            var nonce = reader.ReadUInt64();

            reader.ThrowIfNotEndReached();

            return new PongMessage(nonce);
        }
        #endregion

        #region Instance members
        public UInt64 Nonce { get; private set; }

        public PongMessage(UInt64 nonce)
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
