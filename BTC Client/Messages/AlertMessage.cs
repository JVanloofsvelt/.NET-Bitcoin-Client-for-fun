using System;
using System.Linq;

namespace BTCClient.Messages
{
    class AlertMessage : Message
    {
        #region Static members
        public const string Command = "alert";

        static readonly byte[] FinalAlertHash = new byte[] { 0x66, 0x4B, 0x41, 0x6F };

        public static AlertMessage Parse(byte[] payload)
        {
            // Compute checksum
            var checksum = DoubleSHA256.ComputeChecksum(payload);


            // Parse
            var reader = new PayloadReader(payload);
            
            var alertSize = reader.ReadVarInt();

            var alertFormatVersion = reader.ReadInt32();
            var relayUntil = reader.ReadDateTimeOffset(longer: true);
            var expiration = reader.ReadDateTimeOffset(longer: true);
            var alertID = reader.ReadInt32();

            var cancel0 = reader.ReadInt32();
            var nCancel = reader.ReadVarInt() + 1;
            var toCancel = new Int32[nCancel];

            toCancel[0] = cancel0;

            for (ulong i = 1; i < nCancel; i++)
                toCancel[i] = reader.ReadInt32();

            var minVersion = reader.ReadInt32();
            var maxVersion = reader.ReadInt32();

            var nSubVersions = reader.ReadVarInt();
            var subVersions = new string[nSubVersions];

            for (ulong j = 0; j < nSubVersions; j++)
                subVersions[j] = reader.ReadString();

            var priority = reader.ReadInt32();
            var comment = reader.ReadString();
            var statusBar = reader.ReadString();
            var reserved = reader.ReadString();

            var signature = reader.ReadArray(72);

            reader.ThrowIfNotEndReached();

            // Return
            return new AlertMessage {
                IsFinalAlert = checksum.SequenceEqual(FinalAlertHash),
                StatusBar = statusBar
            };
        }
        #endregion

        #region Instance members
        public bool IsFinalAlert { get; private set; }

        public string StatusBar { get; private set; }

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
