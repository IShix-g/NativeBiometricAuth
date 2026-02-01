#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace NativeBiometricAuth
{
    internal sealed class IosBiometricAuth : IBiometricAuth
    {
        [DllImport("__Internal")]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern bool AuthenticateWithOptions([MarshalAs(UnmanagedType.I1)] bool allowDeviceCredential);

        [DllImport("__Internal")]
        static extern int AuthenticateWithOptionsAndGetReason([MarshalAs(UnmanagedType.I1)] bool allowDeviceCredential);
        
        [DllImport("__Internal")]
        static extern int BiometricAvailability();
        
        [DllImport("__Internal")]
        static extern int DeviceCredentialAvailability();

        public void Authenticate(Action onSuccess, Action<BiometricFailureReason> onFailure, bool allowDeviceCredential)
        {
            var context = SynchronizationContext.Current;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var reason = AuthenticateWithOptionsAndGetReason(allowDeviceCredential);
                if (context != null)
                {
                    context.Post(__ => InvokeResult(reason, onSuccess, onFailure), null);
                }
                else
                {
                    InvokeResult(reason, onSuccess, onFailure);
                }
            });
        }
        
        public BiometricAvailabilityStatus GetAvailability()
        {
            var biometrics = (BiometricAvailability) BiometricAvailability();
            var deviceCredential = (BiometricAvailability) DeviceCredentialAvailability();
            return new BiometricAvailabilityStatus(biometrics, deviceCredential);
        }

        static void InvokeResult(int reason, Action onSuccess, Action<BiometricFailureReason> onFailure)
        {
            if (reason == 0)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFailure?.Invoke((BiometricFailureReason) reason);
            }
        }
    }
}
#endif
