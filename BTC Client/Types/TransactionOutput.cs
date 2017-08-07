using System;

namespace BTCClient
{
    public class TransactionOutput
    {
        const int MaxScriptLength = 10000;

        /// <summary>
        /// Number of satoshis to spend. May be zero; the sum of all outputs may not exceed the sum of satoshis previously spent to the outpoints provided in the input section.
        /// </summary>
        public Int64 Value { get; private set; }

        /// <summary>
        /// Defines the conditions which must be satisfied to spend this output.
        /// </summary>
        public string PublicKeyScript { get; private set; }

        public TransactionOutput(Int64 value, string publicKeyScript)
        {
            if (publicKeyScript.Length > MaxScriptLength)
                throw new ArgumentTooLongException(nameof(publicKeyScript), MaxScriptLength);

            Value = value;
            PublicKeyScript = publicKeyScript;
        }
    }
}
