using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math;
using System.Linq;
using System;

namespace BTCClient
{
    public class ECDSA
    {
        const int PrivateKeySize = 32;

        static readonly ECDomainParameters ECDomainParameters;

        static ECDSA()
        {
            var curveParameters = SecNamedCurves.GetByName("secp256k1");

            ECDomainParameters = new ECDomainParameters
            (
                    curveParameters.Curve,
                    curveParameters.G,
                    curveParameters.N,
                    curveParameters.H
            );
        }

        public static void GenerateKeyPair(out byte[] privateKey, out byte[] publicKey, bool compressedPublicKey)
        {
            var prng = new SecureRandom(); // Probably pseudorandom
            var parameters = new ECKeyGenerationParameters(ECDomainParameters, prng);

            var generator = new ECKeyPairGenerator("ECDSA");
            generator.Init(parameters);
            
            var pair = generator.GenerateKeyPair();

            privateKey = ((ECPrivateKeyParameters)pair.Private).D.ToByteArray();
            publicKey = ((ECPublicKeyParameters)pair.Public).Q.GetEncoded(compressedPublicKey);
        }

        public static byte[] GeneratePublicKey(byte[] privateKey, bool compressed)
        {
            if (privateKey.Length != PrivateKeySize)
                throw new ArgumentException($"Value should contain {PrivateKeySize} bytes", nameof(privateKey));

            return ECDomainParameters.G.Multiply(new BigInteger(privateKey)).GetEncoded(compressed);
        }
    }
}
