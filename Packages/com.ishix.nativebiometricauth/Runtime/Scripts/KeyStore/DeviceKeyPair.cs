
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

namespace NativeBiometricAuth
{
    internal static class DeviceKeyPair
    {
        const string _defaultKeyId = "com.ishix.nativebiometricauth.devicekeypair";
        public static bool AllowInsecureFallback { get; set; } = true;
        static bool s_fallbackWarningLogged;

        public static bool IsSupported
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return true;
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return true;
#elif UNITY_ANDROID && !UNITY_EDITOR
                return AndroidIsSupported() || AllowInsecureFallback;
#elif UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static bool HasKeyPair()
            => HasKeyPair(_defaultKeyId);

        public static bool HasKeyPair(string keyId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return DKP_HasKeyPairForTag(NormalizeKeyId(keyId));
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return MacHasKeyPair(NormalizeKeyId(keyId));
#elif UNITY_ANDROID && !UNITY_EDITOR
            return AndroidHasKeyPair(NormalizeKeyId(keyId));
#else
            return LocalHasKeyPair(NormalizeKeyId(keyId));
#endif
        }

        public static string GetOrCreatePublicKeyBase64()
            => GetOrCreatePublicKeyBase64(_defaultKeyId);

        public static string GetOrCreatePublicKeyBase64(string keyId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            var normalized = NormalizeKeyId(keyId);
            return NativeGetString(() => DKP_GetOrCreatePublicKeyBase64ForTag(normalized));
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return MacGetOrCreatePublicKeyBase64(NormalizeKeyId(keyId));
#elif UNITY_ANDROID && !UNITY_EDITOR
            return AndroidGetOrCreatePublicKeyBase64(NormalizeKeyId(keyId));
#else
            return LocalGetOrCreatePublicKeyBase64(NormalizeKeyId(keyId));
#endif
        }

        public static string GetPublicKeyBase64()
            => GetPublicKeyBase64(_defaultKeyId);

        public static string GetPublicKeyBase64(string keyId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            var normalized = NormalizeKeyId(keyId);
            return NativeGetString(() => DKP_GetPublicKeyBase64ForTag(normalized));
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return MacGetPublicKeyBase64(NormalizeKeyId(keyId));
#elif UNITY_ANDROID && !UNITY_EDITOR
            return AndroidGetPublicKeyBase64(NormalizeKeyId(keyId));
#else
            return LocalGetPublicKeyBase64(NormalizeKeyId(keyId));
#endif
        }

        public static string GetKeyAlgorithm()
            => GetKeyAlgorithm(_defaultKeyId);

        public static string GetKeyAlgorithm(string keyId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return "EC";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return "EC";
#elif UNITY_ANDROID && !UNITY_EDITOR
            return AndroidGetKeyAlgorithm(NormalizeKeyId(keyId));
#else
            return "RSA";
#endif
        }

        public static byte[] Sign(byte[] data)
            => Sign(data, _defaultKeyId);

        public static byte[] Sign(byte[] data, string keyId)
        {
            if (data == null || data.Length == 0)
            {
                return Array.Empty<byte>();
            }

#if UNITY_IOS && !UNITY_EDITOR
            var normalized = NormalizeKeyId(keyId);
            return Convert.FromBase64String(NativeGetString(() => DKP_SignBase64ForTag(normalized, data, data.Length)) ?? string.Empty);
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return MacSign(NormalizeKeyId(keyId), data);
#elif UNITY_ANDROID && !UNITY_EDITOR
            return AndroidSign(NormalizeKeyId(keyId), data);
#else
            return LocalSign(NormalizeKeyId(keyId), data);
#endif
        }

        public static void DeleteKeyPair()
            => DeleteKeyPair(_defaultKeyId);

        public static void DeleteKeyPair(string keyId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            DKP_DeleteKeyPairForTag(NormalizeKeyId(keyId));
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            MacDeleteKeyPair(NormalizeKeyId(keyId));
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidDeleteKeyPair(NormalizeKeyId(keyId));
#else
            LocalDeleteKeyPair(NormalizeKeyId(keyId));
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] static extern bool DKP_HasKeyPairForTag([MarshalAs(UnmanagedType.LPStr)] string tag);
        [DllImport("__Internal")] static extern IntPtr DKP_GetOrCreatePublicKeyBase64ForTag([MarshalAs(UnmanagedType.LPStr)] string tag);
        [DllImport("__Internal")] static extern IntPtr DKP_GetPublicKeyBase64ForTag([MarshalAs(UnmanagedType.LPStr)] string tag);
        [DllImport("__Internal")] static extern IntPtr DKP_SignBase64ForTag([MarshalAs(UnmanagedType.LPStr)] string tag, byte[] data, int length);
        [DllImport("__Internal")] static extern void DKP_DeleteKeyPairForTag([MarshalAs(UnmanagedType.LPStr)] string tag);
        [DllImport("__Internal")] static extern void DKP_FreeString(IntPtr str);
#endif

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        [DllImport("DeviceKeyPair")] static extern bool DKP_HasKeyPairForTag([MarshalAs(UnmanagedType.LPStr)] string tag);
        [DllImport("DeviceKeyPair")] static extern IntPtr DKP_GetOrCreatePublicKeyBase64ForTag([MarshalAs(UnmanagedType.LPStr)] string tag);
        [DllImport("DeviceKeyPair")] static extern IntPtr DKP_GetPublicKeyBase64ForTag([MarshalAs(UnmanagedType.LPStr)] string tag);
        [DllImport("DeviceKeyPair")] static extern IntPtr DKP_SignBase64ForTag([MarshalAs(UnmanagedType.LPStr)] string tag, byte[] data, int length);
        [DllImport("DeviceKeyPair")] static extern void DKP_DeleteKeyPairForTag([MarshalAs(UnmanagedType.LPStr)] string tag);
        [DllImport("DeviceKeyPair")] static extern void DKP_FreeString(IntPtr str);
#endif

#if UNITY_IOS && !UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        static string NativeGetString(Func<IntPtr> call)
        {
            var ptr = call();
            if (ptr == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
            finally
            {
                DKP_FreeString(ptr);
            }
        }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        const string _androidClassName = "com.ishix.nativebiometricauth.devicekeypair.DeviceKeyPairPlugin";

        static AndroidJavaClass AndroidPluginClass => new(_androidClassName);

        static bool AndroidHasKeyPair(string keyId)
        {
            if (!AndroidIsSupported())
            {
                if (AllowInsecureFallback)
                {
                    LogInsecureFallbackWarning();
                    return LocalHasKeyPair(keyId);
                }
                return false;
            }
            using var plugin = AndroidPluginClass;
            return plugin.CallStatic<bool>("hasKeyPair", keyId);
        }

        static string AndroidGetOrCreatePublicKeyBase64(string keyId)
        {
            if (!AndroidIsSupported())
            {
                if (AllowInsecureFallback)
                {
                    LogInsecureFallbackWarning();
                    return LocalGetOrCreatePublicKeyBase64(keyId);
                }
                return null;
            }
            using var plugin = AndroidPluginClass;
            return plugin.CallStatic<string>("getOrCreatePublicKeyBase64", keyId);
        }

        static string AndroidGetPublicKeyBase64(string keyId)
        {
            if (!AndroidIsSupported())
            {
                if (AllowInsecureFallback)
                {
                    LogInsecureFallbackWarning();
                    return LocalGetPublicKeyBase64(keyId);
                }
                return null;
            }
            using var plugin = AndroidPluginClass;
            return plugin.CallStatic<string>("getPublicKeyBase64", keyId);
        }

        static string AndroidGetKeyAlgorithm(string keyId)
        {
            if (!AndroidIsSupported())
            {
                if (AllowInsecureFallback)
                {
                    LogInsecureFallbackWarning();
                    return "RSA";
                }
                return null;
            }
            using var plugin = AndroidPluginClass;
            return plugin.CallStatic<string>("getKeyAlgorithm", keyId);
        }

        static byte[] AndroidSign(string keyId, byte[] data)
        {
            if (!AndroidIsSupported())
            {
                if (AllowInsecureFallback)
                {
                    LogInsecureFallbackWarning();
                    return LocalSign(keyId, data);
                }
                return Array.Empty<byte>();
            }
            using var plugin = AndroidPluginClass;
            var base64 = plugin.CallStatic<string>("signBase64", keyId, data);
            return string.IsNullOrEmpty(base64) ? Array.Empty<byte>() : Convert.FromBase64String(base64);
        }

        static void AndroidDeleteKeyPair(string keyId)
        {
            if (!AndroidIsSupported())
            {
                if (AllowInsecureFallback)
                {
                    LogInsecureFallbackWarning();
                    LocalDeleteKeyPair(keyId);
                }
                return;
            }
            using var plugin = AndroidPluginClass;
            plugin.CallStatic("deleteKeyPair", keyId);
        }

        static bool AndroidIsSupported()
        {
            using var version = new AndroidJavaClass("android.os.Build$VERSION");
            return version.GetStatic<int>("SDK_INT") >= 18;
        }
#endif

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        static bool MacHasKeyPair(string keyId)
            => DKP_HasKeyPairForTag(keyId);

        static string MacGetOrCreatePublicKeyBase64(string keyId)
            => NativeGetString(() => DKP_GetOrCreatePublicKeyBase64ForTag(keyId));

        static string MacGetPublicKeyBase64(string keyId)
            => NativeGetString(() => DKP_GetPublicKeyBase64ForTag(keyId));

        static byte[] MacSign(string keyId, byte[] data)
        {
            var base64 = NativeGetString(() => DKP_SignBase64ForTag(keyId, data, data.Length));
            return string.IsNullOrEmpty(base64) ? Array.Empty<byte>() : Convert.FromBase64String(base64);
        }

        static void MacDeleteKeyPair(string keyId)
            => DKP_DeleteKeyPairForTag(keyId);
#endif

#if UNITY_EDITOR || UNITY_ANDROID || (!UNITY_IOS && !UNITY_ANDROID)
        // ReSharper disable InconsistentNaming
        [Serializable]
        sealed class RsaPrivateKeyData
        {
            public string modulus;
            public string exponent;
            public string d;
            public string p;
            public string q;
            public string dp;
            public string dq;
            public string inverseQ;
        }
        // ReSharper restore InconsistentNaming

        static bool LocalHasKeyPair(string keyId)
        {
            var publicKeyPref = BuildLocalPublicKeyPref(keyId);
            var privateKeyPref = BuildLocalPrivateKeyPref(keyId);
            return PlayerPrefs.HasKey(publicKeyPref) && PlayerPrefs.HasKey(privateKeyPref);
        }

        static string LocalGetOrCreatePublicKeyBase64(string keyId)
        {
            var existing = LocalGetPublicKeyBase64(keyId);
            if (!string.IsNullOrEmpty(existing) && LoadLocalPrivateKey(keyId) != null)
            {
                return existing;
            }

            using var rsa = RSA.Create(2048);
            var parameters = rsa.ExportParameters(true);
            SaveLocalPrivateKey(keyId, parameters);
            var spki = BuildRsaPublicKeySpkiDer(rsa.ExportParameters(false));
            var publicKeyBase64 = Convert.ToBase64String(spki);
            PlayerPrefs.SetString(BuildLocalPublicKeyPref(keyId), publicKeyBase64);
            PlayerPrefs.Save();
            return publicKeyBase64;
        }

        static string LocalGetPublicKeyBase64(string keyId)
        {
            var publicKeyPref = BuildLocalPublicKeyPref(keyId);
            var existing = PlayerPrefs.GetString(publicKeyPref, null);
            if (!string.IsNullOrEmpty(existing))
            {
                return existing;
            }

            var parameters = LoadLocalPrivateKey(keyId);
            if (parameters == null)
            {
                return null;
            }
            var spki = BuildRsaPublicKeySpkiDer(parameters.Value);
            var publicKeyBase64 = Convert.ToBase64String(spki);
            PlayerPrefs.SetString(publicKeyPref, publicKeyBase64);
            PlayerPrefs.Save();
            return publicKeyBase64;
        }

        static byte[] LocalSign(string keyId, byte[] data)
        {
            var parameters = LoadLocalPrivateKey(keyId);
            if (parameters == null)
            {
                return Array.Empty<byte>();
            }

            using var rsa = RSA.Create();
            rsa.ImportParameters(parameters.Value);
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }

        static void LocalDeleteKeyPair(string keyId)
        {
            PlayerPrefs.DeleteKey(BuildLocalPublicKeyPref(keyId));
            PlayerPrefs.DeleteKey(BuildLocalPrivateKeyPref(keyId));
            PlayerPrefs.Save();
        }

        static string BuildLocalPublicKeyPref(string keyId)
            => $"DeviceKeyPair.{keyId}.PublicKey";

        static string BuildLocalPrivateKeyPref(string keyId)
            => $"DeviceKeyPair.{keyId}.PrivateKey";

        static RSAParameters? LoadLocalPrivateKey(string keyId)
        {
            var privateKeyPref = BuildLocalPrivateKeyPref(keyId);
            var serialized = PlayerPrefs.GetString(privateKeyPref, null);
            if (string.IsNullOrEmpty(serialized))
            {
                return null;
            }
            try
            {
                var data = JsonUtility.FromJson<RsaPrivateKeyData>(serialized);
                if (data == null || string.IsNullOrEmpty(data.modulus))
                {
                    return null;
                }
                return new RSAParameters
                {
                    Modulus = Convert.FromBase64String(data.modulus),
                    Exponent = Convert.FromBase64String(data.exponent),
                    D = Convert.FromBase64String(data.d),
                    P = Convert.FromBase64String(data.p),
                    Q = Convert.FromBase64String(data.q),
                    DP = Convert.FromBase64String(data.dp),
                    DQ = Convert.FromBase64String(data.dq),
                    InverseQ = Convert.FromBase64String(data.inverseQ)
                };
            }
            catch
            {
                return null;
            }
        }

        static void SaveLocalPrivateKey(string keyId, RSAParameters parameters)
        {
            var data = new RsaPrivateKeyData
            {
                modulus = Convert.ToBase64String(parameters.Modulus),
                exponent = Convert.ToBase64String(parameters.Exponent),
                d = Convert.ToBase64String(parameters.D),
                p = Convert.ToBase64String(parameters.P),
                q = Convert.ToBase64String(parameters.Q),
                dp = Convert.ToBase64String(parameters.DP),
                dq = Convert.ToBase64String(parameters.DQ),
                inverseQ = Convert.ToBase64String(parameters.InverseQ)
            };
            var serialized = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(BuildLocalPrivateKeyPref(keyId), serialized);
        }

        static byte[] BuildRsaPublicKeySpkiDer(RSAParameters parameters)
        {
            var rsaPublicKey = BuildRsaPublicKeyDer(parameters);
            var algId = BuildAlgorithmIdentifier();
            var bitString = new List<byte>(rsaPublicKey.Length + 5) { 0x03 };
            bitString.AddRange(EncodeLength(rsaPublicKey.Length + 1));
            bitString.Add(0x00);
            bitString.AddRange(rsaPublicKey);
            var contentLength = algId.Length + bitString.Count;
            var result = new List<byte>(contentLength + 8) { 0x30 };
            result.AddRange(EncodeLength(contentLength));
            result.AddRange(algId);
            result.AddRange(bitString);
            return result.ToArray();
        }

        static byte[] BuildAlgorithmIdentifier()
        {
            var oid = new byte[] { 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01 };
            var nullParam = new byte[] { 0x05, 0x00 };
            var contentLength = oid.Length + nullParam.Length;
            var result = new List<byte>(contentLength + 4) { 0x30 };
            result.AddRange(EncodeLength(contentLength));
            result.AddRange(oid);
            result.AddRange(nullParam);
            return result.ToArray();
        }

        static byte[] BuildRsaPublicKeyDer(RSAParameters parameters)
        {
            var modulus = TrimLeadingZeroes(parameters.Modulus);
            var exponent = TrimLeadingZeroes(parameters.Exponent);
            var modulusEncoded = EncodeInteger(modulus);
            var exponentEncoded = EncodeInteger(exponent);
            var contentLength = modulusEncoded.Length + exponentEncoded.Length;
            var result = new List<byte>(contentLength + 8) { 0x30 };
            result.AddRange(EncodeLength(contentLength));
            result.AddRange(modulusEncoded);
            result.AddRange(exponentEncoded);
            return result.ToArray();
        }

        static byte[] EncodeInteger(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return new byte[] { 0x02, 0x01, 0x00 };
            }
            var needsLeadingZero = (value[0] & 0x80) != 0;
            var length = value.Length + (needsLeadingZero ? 1 : 0);
            var output = new List<byte>(length + 2) { 0x02 };
            output.AddRange(EncodeLength(length));
            if (needsLeadingZero)
            {
                output.Add(0x00);
            }
            output.AddRange(value);
            return output.ToArray();
        }

        static byte[] TrimLeadingZeroes(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                return Array.Empty<byte>();
            }
            var offset = 0;
            while (offset < value.Length - 1 && value[offset] == 0x00)
            {
                offset++;
            }
            if (offset == 0)
            {
                return value;
            }
            var trimmed = new byte[value.Length - offset];
            Buffer.BlockCopy(value, offset, trimmed, 0, trimmed.Length);
            return trimmed;
        }

        static IEnumerable<byte> EncodeLength(int length)
        {
            if (length < 0x80)
            {
                return new[] { (byte)length };
            }
            var bytes = new List<byte>();
            var remaining = length;
            while (remaining > 0)
            {
                bytes.Insert(0, (byte)(remaining & 0xFF));
                remaining >>= 8;
            }
            bytes.Insert(0, (byte)(0x80 | bytes.Count));
            return bytes;
        }
#endif

        static string NormalizeKeyId(string keyId)
            => string.IsNullOrEmpty(keyId) ? _defaultKeyId : keyId;

        static void LogInsecureFallbackWarning()
        {
            if (s_fallbackWarningLogged)
            {
                return;
            }
            s_fallbackWarningLogged = true;
            Debug.LogWarning("[Logify] DeviceKeyPair fallback is using PlayerPrefs storage because Android Keystore is not supported on this device. Set DeviceKeyPair.AllowInsecureFallback = false to disable.");
        }
    }
}
