using System;

namespace BTCClient
{
    enum ObjectType : UInt32
    {
        Transaction,
        Block,
        FilteredBlock
    }

    class InventoryVector
    {
        public ObjectType Type { get; private set; }
        public byte[] Hash { get; private set; }

        public InventoryVector(ObjectType type, byte[] hash)
        {
            if (hash.Length != DoubleSHA256.HashSize)
                throw new InvalidHashSizeException(nameof(hash));

            Type = type;
            Hash = hash;
        }
    }
}
