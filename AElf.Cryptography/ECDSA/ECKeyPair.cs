﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace AElf.Cryptography.ECDSA
{
    public class ECKeyPair
    {
        public static int AddressLength { get; } = 18;
        
        public ECPrivateKeyParameters PrivateKey { get; private set; }
        public ECPublicKeyParameters PublicKey { get; private set; }
        
        public ECKeyPair(ECPrivateKeyParameters privateKey, ECPublicKeyParameters publicKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public byte[] GetEncodedPublicKey(bool compressed = false)
        {
            return PublicKey.Q.GetEncoded(compressed);
        }

        public static ECKeyPair FromPublicKey(byte[] publicKey)
        {
            ECPublicKeyParameters pubKey 
                = new ECPublicKeyParameters(Parameters.Curve.Curve.DecodePoint(publicKey), Parameters.DomainParams);
            
            ECKeyPair k = new ECKeyPair(null, pubKey);

            return k;
        }

        public byte[] GetAddress()
        {
            return this.GetEncodedPublicKey().Take(AddressLength).ToArray();
        }

        public string GetBase64Address()
        {
            return Convert.ToBase64String(GetAddress());
        }
    }
}