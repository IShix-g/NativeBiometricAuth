
using System;

namespace NativeBiometricAuth
{
    internal interface IBiometricAuth
    {
        void Authenticate(Action onSuccess, Action<BiometricFailureReason> onFailure, bool allowDeviceCredential);
        BiometricAvailabilityStatus GetAvailability();
    }
}
