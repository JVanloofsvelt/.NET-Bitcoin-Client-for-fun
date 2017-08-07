using System;

namespace BTCClient
{
    class Block
    {
        public Int32 Version { get; private set; }
        public byte[] PreviousBlockHash { get; private set; }
        public byte[] MerkleRoot { get; private set; }
        public UInt32 Timestamp { get; private set; }
        public UInt32 DifficultyTarget { get; private set; }
        public UInt32 Nonce { get; private set; }
        public Transaction[] Transactions { get; private set; }
    }
}
