
using System;
using UnityEngine;

namespace NativeBiometricAuth
{
    internal sealed class UnsupportedBiometricAuth : IBiometricAuth
    {
        public void Authenticate(Action onSuccess, Action<BiometricFailureReason> onFailure, bool allowDeviceCredential)
        {
            Debug.LogError("Biometric authentication is not supported on this platform.");
            onFailure?.Invoke(BiometricFailureReason.NotSupported);
        }
        
        public BiometricAvailabilityStatus GetAvailability()
            => new BiometricAvailabilityStatus(BiometricAvailability.NotSupported, BiometricAvailability.NotSupported);
    }
}
