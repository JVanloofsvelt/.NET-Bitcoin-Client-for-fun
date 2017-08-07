using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BTCClient;

namespace Tests
{
    [TestClass]
    public class TransactionTests
    {
        [TestMethod]
        public void TestCreation()
        {
            var previousOutput = new TransactionInput.OutPoint(
                transactionHash: new byte[] { 0x48, 0x4d, 0x40, 0xd4, 0x5b, 0x9e, 0xa0, 0xd6, 0x52, 0xfc, 0xa8, 0x25, 0x8a, 0xb7, 0xca, 0xa4, 0x25, 0x41, 0xeb, 0x52, 0x97, 0x58, 0x57, 0xf9, 0x6f, 0xb5, 0x0c, 0xd7, 0x32, 0xc8, 0xb4, 0x81 },
                index: 0
            );

            var input = new TransactionInput(previousOutput, "");
            var output = new TransactionOutput(91234, "");

            var transaction = new Transaction(
                new[] { input },
                new[] { output }
            );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMinimumLockTime()
        {
            new Transaction(null, null, DateTimeOffset.FromUnixTimeSeconds(Transaction.MinimumTimestamp - 1));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMaximumBlockNumer()
        {
            new Transaction(null, null, Transaction.MinimumTimestamp);
        }
    }
}
