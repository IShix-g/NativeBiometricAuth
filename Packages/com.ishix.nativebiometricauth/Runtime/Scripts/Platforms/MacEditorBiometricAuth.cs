#if UNITY_EDITOR_OSX
using System;
using System.Runtime.InteropServices;

namespace NativeBiometricAuth
{
    internal sealed class MacEditorBiometricAuth : IBiometricAuth
    {
        [DllImport("NativeBiometricAuth", EntryPoint = "AuthenticateWithOptions")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern bool AuthenticateEditor([MarshalAs(UnmanagedType.I1)] bool allowDeviceCredential);

        [DllImport("NativeBiometricAuth", EntryPoint = "AuthenticateWithOptionsAndGetReason")]
        static extern int AuthenticateEditorWithReason([MarshalAs(UnmanagedType.I1)] bool allowDeviceCredential);

        [DllImport("NativeBiometricAuth", EntryPoint = "BiometricAvailability")]
        static extern int BiometricAvailability();

        [DllImport("NativeBiometricAuth", EntryPoint = "DeviceCredentialAvailability")]
        static extern int DeviceCredentialAvailability();

        public void Authenticate(Action onSuccess, Action<BiometricFailureReason> onFailure, bool allowDeviceCredential)
        {
            var reason = AuthenticateEditorWithReason(allowDeviceCredential);
            if (reason == 0)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke((BiometricFailureReason) reason);
            }
        }

        public BiometricAvailabilityStatus GetAvailability()
        {
            var biometrics = (BiometricAvailability) BiometricAvailability();
            var deviceCredential = (BiometricAvailability) DeviceCredentialAvailability();
            return new BiometricAvailabilityStatus(biometrics, deviceCredential);
        }
    }
}
#endif
