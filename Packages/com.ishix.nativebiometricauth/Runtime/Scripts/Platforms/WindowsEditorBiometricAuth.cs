#if UNITY_EDITOR_WIN
using System;
using UnityEditor;

namespace NativeBiometricAuth
{
    internal sealed class WindowsEditorBiometricAuth : IBiometricAuth
    {
        public void Authenticate(Action onSuccess, Action<BiometricFailureReason> onFailure, bool allowDeviceCredential)
        {
            var result = EditorUtility.DisplayDialog(
                "Biometric Authentication",
                "[Test] Simulate biometric authentication success?",
                "Success",
                "Failure"
            );

            if (result)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke(BiometricFailureReason.AuthenticationFailed);
            }
        }

        public BiometricAvailabilityStatus GetAvailability()
            => new (BiometricAvailability.SupportedConfigured, BiometricAvailability.SupportedConfigured);
    }
}
#endif
