using System;

namespace BTCClient
{
    public class TransactionInput
    {
        const int MaxScriptLength = 10000;

        /// <summary>
        /// The previous outpoint being spent.
        /// </summary>
        public OutPoint PreviousOutput { get; private set; }

        /// <summary>
        /// A script which satisfies the conditions placed in the outpoint’s public key script. Should only contain data pushes; see:
        /// https://bitcoin.org/en/developer-reference#signature_script_modification_warning
        /// </summary>
        public string SignatureScript { get; private set; }

        /// <summary>
        /// Sequence number. Default for Bitcoin Core and almost all other programs is 0xFFFFFFFF.
        /// </summary>
        public UInt32 Sequence { get; private set; }

        public TransactionInput(OutPoint previousOutput, string signatureScript, UInt32 sequence= 0xFFFFFFFF)
        {
            if (signatureScript.Length > MaxScriptLength)
                throw new ArgumentTooLongException(nameof(signatureScript), MaxScriptLength);

            PreviousOutput = previousOutput;
            SignatureScript = signatureScript;
            Sequence = sequence;
        }

        public class OutPoint
        {
            /// <summary>
            /// The TxID of the transaction holding the output to spend. The TxID is a hash provided here in internal byte order (little endian).
            /// </summary>
            public byte[] TransactionHash { get; private set; }

            /// <summary>
            /// The output index number of the specific output to spend from the transaction. The first output is 0x00000000.
            /// </summary>
            public UInt32 Index { get; private set; }

            public OutPoint(byte[] transactionHash, UInt32 index)
            {
                if (transactionHash.Length != DoubleSHA256.HashSize)
                    throw new InvalidHashSizeException(nameof(transactionHash));

                TransactionHash = transactionHash;
                Index = index;
            }
        }
    }
}
