using System;

namespace BTCClient
{
    public class Transaction
    {
        public const int MinimumTimestamp = 500000000;  // Five hundred million

        static readonly DateTimeOffset MinimumLockTime = DateTimeOffset.FromUnixTimeSeconds(MinimumTimestamp);
        static readonly UInt32 MaximumBlockNumber = MinimumTimestamp - 1;  // Any value below the minimum timestamp is treated as a block number


        /// <summary>
        /// Transaction version number. Programs creating transactions using newer consensus rules may use higher version numbers.
        /// </summary>
        public Int32 Version { get; private set; } = 1;

        public TransactionInput[] Inputs { get; private set; }

        public TransactionOutput[] Outputs { get; private set; }

        /// <summary>
        /// Timestamp at which this transaction is unlocked. Ignored if null.
        /// </summary>
        public DateTimeOffset? LockTime
        {
            get
            {
                if (lockTime >= MinimumTimestamp)
                    return DateTimeOffset.FromUnixTimeSeconds(lockTime);
                else
                    return null;
            }
        }

        /// <summary>
        /// Block number at which this transaction is unlocked. Ignored if null.
        /// </summary>
        public UInt32? MinimumBlockNumber
        {
            get
            {
                if (lockTime < MinimumTimestamp)
                    return lockTime;
                else
                    return null;
            }
        }

        public UInt32 lockTime { get; private set; }

        private void Init(TransactionInput[] inputs, TransactionOutput[] outputs, UInt32 lockTime)
        {
            this.lockTime = lockTime;
            Inputs = inputs;
            Outputs = outputs;
        }

        public Transaction(TransactionInput[] inputs, TransactionOutput[] outputs)
        {
            Init(inputs, outputs, lockTime: 0);
        }

        /// <summary>
        /// </summary>
        /// <param name="minimumBlockNumber">Block number at which this transaction is unlocked. Ignored if null.</param>
        public Transaction(TransactionInput[] inputs, TransactionOutput[] outputs, UInt32 minimumBlockNumber)
        {
            if (minimumBlockNumber > MaximumBlockNumber)
                throw new ArgumentException($"Value should be lesser than {MaximumBlockNumber + 1 }", nameof(minimumBlockNumber));

            Init(inputs, outputs, lockTime: minimumBlockNumber);
        }

        /// <summary>
        /// </summary>
        /// <param name="lockTime">Timestamp at which this transaction is unlocked. Ignored if null.</param>
        public Transaction(TransactionInput[] inputs, TransactionOutput[] outputs, DateTimeOffset lockTime)
        {
            if (lockTime < MinimumLockTime)
                throw new ArgumentException($"Value should be greater than or equal to {MinimumLockTime.ToUniversalTime()} UTC", nameof(lockTime));

            var timestamp = Convert.ToUInt32(lockTime.ToUnixTimeSeconds());

            Init(inputs, outputs, lockTime: timestamp);
        }
    }
}
