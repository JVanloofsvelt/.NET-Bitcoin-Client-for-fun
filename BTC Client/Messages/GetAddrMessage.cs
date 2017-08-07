using System;

namespace BTCClient.Messages
{
    class GetAddrMessage : Message
    {
        #region Static members
        public const string Command = "getaddr";

        public static GetAddrMessage Parse(byte[] payload)
        {
            if ((payload?.Length ?? 0) != 0)
                throw new Exception($"Payload given, but a {nameof(GetAddrMessage)} should not contain a payload");
            
            return new GetAddrMessage();
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
