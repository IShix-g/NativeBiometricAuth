
namespace NativeBiometricAuth
{
    public sealed class BiometricActivationKeyStoreProvider : IBiometricActivationProvider
    {
        const string _defaultKeyId = "com.ishix.nativebiometricauth.activation";
        readonly string _keyId;

        public BiometricActivationKeyStoreProvider(string keyId = _defaultKeyId)
            => _keyId = string.IsNullOrEmpty(keyId)
                ? _defaultKeyId
                : keyId;

        public bool TryGet(out bool isActive)
        {
            isActive = DeviceKeyPair.HasKeyPair(_keyId);
            return true;
        }

        public void Set(bool isActive)
        {
            if (isActive)
            {
                DeviceKeyPair.GetOrCreatePublicKeyBase64(_keyId);
            }
            else
            {
                DeviceKeyPair.DeleteKeyPair(_keyId);
            }
        }
    }
}
