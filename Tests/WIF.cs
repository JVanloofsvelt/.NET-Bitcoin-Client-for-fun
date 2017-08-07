using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BTCClient.Encoding;

namespace Tests
{
    [TestClass]
    public class WIFTests
    {
        [TestMethod]
        public void TestEncodeDecode()
        {
            var key = Utils.HexStringToBytes("F19C523315891E6E15AE0608A35EEC2E00EBD6D1984CF167F46336DABD9B2DE4");
            var wif = "5KehCbbxxMsPomgbYqJf2VXKtiD8UKVuaHStjaUyRsZ1X2KjmFZ";

            var wifEncoded = WIF.Encode(key);
            Assert.AreEqual(wif, wifEncoded);

            var keyDecoded = WIF.Decode(wif);
            CollectionAssert.AreEqual(key, keyDecoded);
        }
    }
}
