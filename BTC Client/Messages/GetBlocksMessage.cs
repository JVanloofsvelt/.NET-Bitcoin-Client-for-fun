using System;

namespace BTCClient.Messages
{
    class GetBlocksMessage : Message
    {
        #region Static members
        public const string Command = "getblocks";

        public GetBlocksMessage Parse(byte[] payload)
        {
            var reader = new PayloadReader(payload);

            var version = reader.ReadUInt32();
            var count = reader.ReadVarInt();

            throw new NotImplementedException();

            for (ulong i = 0; i < count; i++)
            {
            }

            reader.ThrowIfNotEndReached();
        }
        #endregion


        #region Instance members
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
}
