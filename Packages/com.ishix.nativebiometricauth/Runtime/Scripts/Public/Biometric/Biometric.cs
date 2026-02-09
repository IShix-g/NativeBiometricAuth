
using System;

namespace NativeBiometricAuth
{
    public static class Biometric
    {
        public static event Action<bool> OnActiveChanged = delegate { };
        public static event Action OnSuccess = delegate { };
        public static event Action<BiometricFailureReason> OnFailure = delegate { };

        public static bool IsActive => GetActive();
        
        static readonly IBiometricAuth s_platform = CreatePlatform();
        static IBiometricActivationProvider s_activationProvider = new BiometricActivationKeyStoreProvider();
        static bool s_hasActive;
        static bool s_isActive;
        static bool s_isTogglingActive;

        public static bool IsAvailable(bool allowDeviceCredential = true)
            => GetActivationState(allowDeviceCredential) == BiometricActivationState.Enabled;
        
        public static BiometricActivationState GetActivationState(bool allowDeviceCredential = true)
        {
            var availability = GetAvailability();
            var biometrics = availability.Biometrics;
            var deviceCredential = availability.DeviceCredential;
            var biometricConfigured = biometrics == BiometricAvailability.SupportedConfigured;
            var biometricNotConfigured = biometrics == BiometricAvailability.SupportedNotConfigured;
            var deviceConfigured = deviceCredential == BiometricAvailability.SupportedConfigured;
            var deviceNotConfigured = deviceCredential == BiometricAvailability.SupportedNotConfigured;

            if (allowDeviceCredential)
            {
                if (biometricConfigured
                    || deviceConfigured)
                {
                    return BiometricActivationState.Enabled;
                }
                if (biometricNotConfigured
                    || deviceNotConfigured)
                {
                    return BiometricActivationState.NotConfigured;
                }
                return BiometricActivationState.Disabled;
            }
            if (biometricConfigured)
            {
                return BiometricActivationState.Enabled;
            }
            if (biometricNotConfigured)
            {
                return BiometricActivationState.NotConfigured;
            }
            return BiometricActivationState.Disabled;
        }
        
        public static BiometricAvailabilityStatus GetAvailability() => s_platform.GetAvailability();
        
        public static void Authenticate(Action onSuccess, Action<BiometricFailureReason> onFailure, bool allowDeviceCredential = true)
        {
            if (!IsActive)
            {
                NotifyFailure(BiometricFailureReason.Inactive, onFailure);
                return;
            }
            if (TryGetPrecheckFailureReason(allowDeviceCredential, out var reason))
            {
                NotifyFailure(reason, onFailure);
                return;
            }
            AuthenticateInternal(onSuccess, onFailure, allowDeviceCredential);
        }

        public static bool SetAndroidAllowWeakBiometrics(bool allowWeakBiometrics)
        {
            if (s_platform is IAndroidBiometricAuth obj)
            {
                obj.AllowWeakBiometrics = allowWeakBiometrics;
                return true;
            }
            return false;
        }

        public static bool TryGetAndroidAllowWeakBiometrics(out bool allowWeakBiometrics)
        {
            if (s_platform is IAndroidBiometricAuth obj)
            {
                allowWeakBiometrics = obj.AllowWeakBiometrics;
                return true;
            }
            allowWeakBiometrics = false;
            return false;
        }
        
        public static void SetActivationProvider(IBiometricActivationProvider provider)
        {
            s_activationProvider = provider ?? new BiometricActivationKeyStoreProvider();
            s_hasActive = false;
        }
        
        static IBiometricAuth CreatePlatform()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return new IosBiometricAuth();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidBiometricAuth();
#elif UNITY_EDITOR_OSX
            return new MacEditorBiometricAuth();
#elif UNITY_STANDALONE_OSX
            return new MacStandaloneBiometricAuth();
#elif UNITY_EDITOR_WIN
            return new WindowsEditorBiometricAuth();
#else
            return new UnsupportedBiometricAuth();
#endif
        }

        static bool GetActive()
        {
            if (!s_hasActive)
            {
                if (!s_activationProvider.TryGet(out s_isActive))
                {
                    s_isActive = true;
                }
                s_hasActive = true;
            }
            return s_isActive;
        }

        public static void SetActive(bool value, bool allowDeviceCredential, bool authenticate = true, Action onSuccess = null, Action<BiometricFailureReason> onFailure = null)
        {
            var current = GetActive();
            if (value && !current)
            {
                if (!authenticate)
                {
                    ApplyActive(true);
                    return;
                }
                if (s_isTogglingActive)
                {
                    return;
                }
                s_isTogglingActive = true;
                AuthenticateInternal(
                    () =>
                    {
                        s_isTogglingActive = false;
                        ApplyActive(true);
                        onSuccess?.Invoke();
                    },
                    reason =>
                    {
                        s_isTogglingActive = false;
                        onFailure?.Invoke(reason);
                    },
                    allowDeviceCredential);
            }
            else if (current != value)
            {
                ApplyActive(value);
            }
        }

        static void ApplyActive(bool value)
        {
            s_isActive = value;
            s_hasActive = true;
            s_activationProvider.Set(value);
            OnActiveChanged(value);
        }
        
        static void AuthenticateInternal(Action onSuccess, Action<BiometricFailureReason> onFailure, bool allowDeviceCredential)
        {
            s_platform.Authenticate(
                () =>
                {
                    OnSuccess();
                    onSuccess?.Invoke();
                },
                reason => NotifyFailure(reason, onFailure),
                allowDeviceCredential);
        }

        static bool TryGetPrecheckFailureReason(bool allowDeviceCredential, out BiometricFailureReason reason)
        {
            var availability = GetAvailability();
            var biometrics = availability.Biometrics;
            var deviceCredential = availability.DeviceCredential;
            var biometricConfigured = biometrics == BiometricAvailability.SupportedConfigured;
            var biometricNotConfigured = biometrics == BiometricAvailability.SupportedNotConfigured;
            var biometricNotSupported = biometrics == BiometricAvailability.NotSupported;
            var deviceConfigured = deviceCredential == BiometricAvailability.SupportedConfigured;
            var deviceNotConfigured = deviceCredential == BiometricAvailability.SupportedNotConfigured;
            var deviceNotSupported = deviceCredential == BiometricAvailability.NotSupported;

            if (allowDeviceCredential)
            {
                if (biometricConfigured || deviceConfigured)
                {
                    reason = default;
                    return false;
                }
                if (biometricNotConfigured || deviceNotConfigured)
                {
                    reason = BiometricFailureReason.NotConfigured;
                    return true;
                }
                if (biometricNotSupported && deviceNotSupported)
                {
                    reason = BiometricFailureReason.NotSupported;
                    return true;
                }
                reason = BiometricFailureReason.SystemError;
                return true;
            }

            if (biometricConfigured)
            {
                reason = default;
                return false;
            }
            if (biometricNotConfigured)
            {
                reason = BiometricFailureReason.NotConfigured;
                return true;
            }
            if (biometricNotSupported)
            {
                reason = BiometricFailureReason.NotSupported;
                return true;
            }
            reason = BiometricFailureReason.SystemError;
            return true;
        }

        static void NotifyFailure(BiometricFailureReason reason, Action<BiometricFailureReason> onFailure)
        {
            OnFailure(reason);
            onFailure?.Invoke(reason);
        }
    }
}
