﻿using System;

namespace BTCClient.Messages
{
    class InventoryMessage : Message
    {
        #region Static members
        public const string Command = "inv";
        
        public static InventoryMessage Parse(byte[] payload)
        {
            var reader = new PayloadReader(payload);

            var count = reader.ReadVarInt();  // TODO: warn when count > 50,000
            var objects = new InventoryVector[count];

            for (ulong i = 0; i < count; i++)
                objects[i] = reader.ReadInventoryVector();

            reader.ThrowIfNotEndReached();

            return new InventoryMessage
            {
                Objects = objects
            };
        }
        #endregion

        #region Instance members
        public InventoryVector[] Objects { get; private set; }

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
