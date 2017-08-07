using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BTCClient;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class ECDSATest
    {
        static readonly byte[] privateKey = Utils.HexStringToBytes("18E14A7B6A307F426A94F8114701E7C8E774E7F9A47E2C2035DB29A206321725");
        static readonly byte[] publicKey = Utils.HexStringToBytes("0450863AD64A87AE8A2FE83C1AF1A8403CB53F53E486D8511DAD8A04887E5B23522CD470243453A299FA9E77237716103ABC11A1DF38855ED6F2EE187E9C582BA6");

        [TestMethod]
        public void TestPublicKeyGeneration()
        {
            var generatedPublicKey = ECDSA.GeneratePublicKey(privateKey, false);
            CollectionAssert.AreEqual(generatedPublicKey, publicKey);
        }

        [TestMethod]
        public void TestKeyPairGeneration()
        {
            byte[] privateKey, publicKey;

            foreach (var compressed in new bool[]{ true, false })
            {
                ECDSA.GenerateKeyPair(out privateKey, out publicKey, compressed);
                var publicKeyCheck = ECDSA.GeneratePublicKey(privateKey, compressed);

                CollectionAssert.AreEqual(publicKey, publicKeyCheck);
            }
        }
    }
}
