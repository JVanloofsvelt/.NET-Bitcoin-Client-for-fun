using System;
using System.Linq;
using System.Security.Cryptography;

namespace BTCClient
{
    public class DoubleSHA256 : IDisposable
    {
        #region Static members
        public const int HashSize = 32;
        public const int ChecksumSize = 4;

        public static byte[] ComputeChecksum(byte[] payload)  // TODO: perhaps add a VerifyChecksum method or something to avoid the array copy
        {
            return ComputeHash(payload).Take(ChecksumSize).ToArray();
        }

        public static byte[] ComputeHash(byte[] payload)
        {
            using (var hasher = new DoubleSHA256())
            {
                return hasher.Compute(payload);
            }
        }
        #endregion

        #region Instance members
        SHA256 sha256;

        private DoubleSHA256()
        {
            sha256 = SHA256.Create();
        }

        public byte[] Compute(byte[] buffer)
        {
            return sha256.ComputeHash(sha256.ComputeHash(buffer));
        }

        public void Dispose()
        {
            // This call to dispose will release managed and unmanaged resources in a predictable order
            Dispose(true);

            // Tell GC that there's no need to finalize this object since it was already disposed of.
            // Since finalizing happens between GC rounds this object can now be disposed of as soon as the next GC round as opposed to the second next round.
            GC.SuppressFinalize(this);
        }

        // Finalizer
        ~DoubleSHA256()
        {
            // This call to dispose will clean up any remaining unmanaged resources in an unpredictable order
            Dispose(false);
        }

        /// <summary>
        /// Should implement all resource cleanup to be shared between Dispose and the (optional) finalizer.
        /// </summary>
        /// <param name="disposing">True when called from Dispose, false when called from the finalizer.</param>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Disposable objects properly wrapped in a using statement are disposed of in a predictable order, hence following resources supposedly still have to be released
                sha256?.Dispose();
            }

            // Code below is executed when disposing as well as when finalizing. Finalizers are called in an unpredictable order;
            // Any referenced disposable objects may or may not have been disposed of already so we shouldnt be touching managed resources from here on out

            // Release remaining unmanaged resources ...
        }
        #endregion
    }
}
