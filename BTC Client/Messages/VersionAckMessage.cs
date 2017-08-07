using System;

namespace BTCClient.Messages
{
    class VerAckMessage : Message
    {
        #region Static members
        public const string Command = "verack";

        public static VerAckMessage Parse(byte[] payload)
        {
            if ((payload?.Length ?? 0) != 0)
                throw new Exception($"Payload given, but a {nameof(GetAddrMessage)} should not contain a payload");

            return new VerAckMessage();
        }
        #endregion

        #region Instance members
        protected override string GetCommand()
        {
            return Command;
        }

        protected override byte[] GetPayload()
        {
            return null;
        }
        #endregion
    }
}
