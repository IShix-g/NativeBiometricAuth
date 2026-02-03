#if UNITY_ANDROID
using System;
using UnityEngine;

namespace NativeBiometricAuth
{
    internal sealed class AndroidBiometricAuth : IBiometricAuth, IAndroidBiometricAuth
    {
        System.Threading.SynchronizationContext _currentThread;
        bool _allowWeakBiometrics = true;

        public void Authenticate(Action onSuccess, Action<BiometricFailureReason> onFailure, bool allowDeviceCredential)
        {
            _currentThread = System.Threading.SynchronizationContext.Current;
            var title = Application.productName;

            new AndroidJavaClass("com.ishix.nativebiometricauth.NativeBiometricAuthActivity").CallStatic("launchFromUnity",
                new OnHostReadyCallback((activity) =>
                {
                    var biometricManagerClass = new AndroidJavaClass("androidx.biometric.BiometricManager");
                    var biometricManager = biometricManagerClass.CallStatic<AndroidJavaObject>("from", activity);
                    var authenticatorsClass = new AndroidJavaClass("androidx.biometric.BiometricManager$Authenticators");
                    var biometricStrong = authenticatorsClass.GetStatic<int>("BIOMETRIC_STRONG");
                    var biometricWeak = authenticatorsClass.GetStatic<int>("BIOMETRIC_WEAK");
                    var deviceCredential = authenticatorsClass.GetStatic<int>("DEVICE_CREDENTIAL");
                    var biometricAuthenticators = _allowWeakBiometrics ? (biometricStrong | biometricWeak) : biometricStrong;
                    var allowedAuthenticators = allowDeviceCredential
                        ? (biometricAuthenticators | deviceCredential)
                        : biometricAuthenticators;
                    var biometricSuccess = biometricManagerClass.GetStatic<int>("BIOMETRIC_SUCCESS");
                    var biometricNoneEnrolled = biometricManagerClass.GetStatic<int>("BIOMETRIC_ERROR_NONE_ENROLLED");
                    var biometricNoHardware = biometricManagerClass.GetStatic<int>("BIOMETRIC_ERROR_NO_HARDWARE");
                    var biometricHwUnavailable = biometricManagerClass.GetStatic<int>("BIOMETRIC_ERROR_HW_UNAVAILABLE");

                    var canAuthenticate = biometricManager.Call<int>("canAuthenticate", allowedAuthenticators);
                    if (canAuthenticate != biometricSuccess)
                    {
                        var reason = MapAuthenticateError(canAuthenticate, biometricNoneEnrolled, biometricNoHardware, biometricHwUnavailable);
                        _currentThread.Post(_ => onFailure?.Invoke(reason), null);
                        return;
                    }

                    var biometricPromptClass = new AndroidJavaClass("androidx.biometric.BiometricPrompt");
                    var errorCanceled = biometricPromptClass.GetStatic<int>("ERROR_CANCELED");
                    var errorUserCanceled = biometricPromptClass.GetStatic<int>("ERROR_USER_CANCELED");
                    var errorNegativeButton = biometricPromptClass.GetStatic<int>("ERROR_NEGATIVE_BUTTON");
                    var errorNoBiometrics = biometricPromptClass.GetStatic<int>("ERROR_NO_BIOMETRICS");
                    var errorNoDeviceCredential = biometricPromptClass.GetStatic<int>("ERROR_NO_DEVICE_CREDENTIAL");
                    var errorHwUnavailable = biometricPromptClass.GetStatic<int>("ERROR_HW_UNAVAILABLE");
                    var errorHwNotPresent = biometricPromptClass.GetStatic<int>("ERROR_HW_NOT_PRESENT");
                    var errorLockout = biometricPromptClass.GetStatic<int>("ERROR_LOCKOUT");
                    var errorLockoutPermanent = biometricPromptClass.GetStatic<int>("ERROR_LOCKOUT_PERMANENT");

                    using var promptInfoBuilder = new AndroidJavaObject("androidx.biometric.BiometricPrompt$PromptInfo$Builder");
                    promptInfoBuilder.Call<AndroidJavaObject>("setTitle", title);
                    promptInfoBuilder.Call<AndroidJavaObject>("setSubtitle", "Authenticate with biometrics");
                    if (allowDeviceCredential)
                    {
                        promptInfoBuilder.Call<AndroidJavaObject>("setAllowedAuthenticators", allowedAuthenticators);
                    }
                    else
                    {
                        promptInfoBuilder.Call<AndroidJavaObject>("setNegativeButtonText", "Cancel");
                    }
                    var promptInfo = promptInfoBuilder.Call<AndroidJavaObject>("build");
                    var authCallback = new AndroidJavaObject("com.ishix.nativebiometricauth.NativeBiometricPromptCallback",
                        new AuthenticationCallback(onSuccess, onFailure, _currentThread,
                            errorCanceled, errorUserCanceled, errorNegativeButton,
                            errorNoBiometrics, errorNoDeviceCredential, errorHwUnavailable, errorHwNotPresent,
                            errorLockout, errorLockoutPermanent));
                    var biometricPrompt = new AndroidJavaObject("androidx.biometric.BiometricPrompt", activity,
                        authCallback);
                    biometricPrompt.Call("authenticate", promptInfo);
                }));
        }
        
        public BiometricAvailabilityStatus GetAvailability()
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity == null)
            {
                return new BiometricAvailabilityStatus(BiometricAvailability.NotSupported, BiometricAvailability.NotSupported);
            }

            using var biometricManagerClass = new AndroidJavaClass("androidx.biometric.BiometricManager");
            var biometricManager = biometricManagerClass.CallStatic<AndroidJavaObject>("from", activity);
            using var authenticatorsClass = new AndroidJavaClass("androidx.biometric.BiometricManager$Authenticators");
            var biometricStrong = authenticatorsClass.GetStatic<int>("BIOMETRIC_STRONG");
            var biometricWeak = authenticatorsClass.GetStatic<int>("BIOMETRIC_WEAK");
            var deviceCredential = authenticatorsClass.GetStatic<int>("DEVICE_CREDENTIAL");
            var biometricSuccess = biometricManagerClass.GetStatic<int>("BIOMETRIC_SUCCESS");
            var biometricNoneEnrolled = biometricManagerClass.GetStatic<int>("BIOMETRIC_ERROR_NONE_ENROLLED");
            var biometricNoHardware = biometricManagerClass.GetStatic<int>("BIOMETRIC_ERROR_NO_HARDWARE");
            var biometricHwUnavailable = biometricManagerClass.GetStatic<int>("BIOMETRIC_ERROR_HW_UNAVAILABLE");

            var biometricAuthenticator = _allowWeakBiometrics ? biometricWeak : biometricStrong;
            var biometricStatus = biometricManager.Call<int>("canAuthenticate", biometricAuthenticator);
            var biometrics = MapAndroidBiometricStatus(biometricStatus, biometricSuccess, biometricNoneEnrolled,
                biometricNoHardware, biometricHwUnavailable);

            var deviceCredentialStatus = GetDeviceCredentialStatus(activity, biometricManager, deviceCredential,
                biometricSuccess, biometricNoneEnrolled);
            var deviceCredentialAvailability = MapAndroidBiometricStatus(deviceCredentialStatus, biometricSuccess,
                biometricNoneEnrolled, biometricNoHardware, biometricHwUnavailable);

            return new BiometricAvailabilityStatus(biometrics, deviceCredentialAvailability);
        }

        public bool AllowWeakBiometrics
        {
            get => _allowWeakBiometrics;
            set => _allowWeakBiometrics = value;
        }
        
        static BiometricAvailability MapAndroidBiometricStatus(int status, int success, int noneEnrolled, int noHardware, int hwUnavailable)
        {
            if (status == success)
            {
                return BiometricAvailability.SupportedConfigured;
            }
            if (status == noneEnrolled)
            {
                return BiometricAvailability.SupportedNotConfigured;
            }
            if (status == noHardware || status == hwUnavailable)
            {
                return BiometricAvailability.NotSupported;
            }
            return BiometricAvailability.NotSupported;
        }
        
        static int GetDeviceCredentialStatus(AndroidJavaObject activity, AndroidJavaObject biometricManager, int deviceCredential,
            int success, int noneEnrolled)
        {
            using var versionClass = new AndroidJavaClass("android.os.Build$VERSION");
            var sdkInt = versionClass.GetStatic<int>("SDK_INT");
            if (sdkInt >= 30)
            {
                return biometricManager.Call<int>("canAuthenticate", deviceCredential);
            }

            using var keyguardManager = activity.Call<AndroidJavaObject>("getSystemService", "keyguard");
            if (keyguardManager == null)
            {
                return noneEnrolled;
            }

            var isDeviceSecure = keyguardManager.Call<bool>("isDeviceSecure");
            return isDeviceSecure ? success : noneEnrolled;
        }

        sealed class OnHostReadyCallback : AndroidJavaProxy
        {
            readonly Action<AndroidJavaObject> _callback;
            public OnHostReadyCallback(Action<AndroidJavaObject> callback)
                : base("com.ishix.nativebiometricauth.NativeBiometricAuthActivity$OnHostReadyCallback")
            {
                _callback = callback;
            }
            public void onHostReady(AndroidJavaObject activity) => _callback?.Invoke(activity);
        }

        sealed class AuthenticationCallback : AndroidJavaProxy
        {
            readonly Action _onSuccess;
            readonly Action<BiometricFailureReason> _onFailure;
            readonly System.Threading.SynchronizationContext _thread;
            readonly int _errorCanceled;
            readonly int _errorUserCanceled;
            readonly int _errorNegativeButton;
            readonly int _errorNoBiometrics;
            readonly int _errorNoDeviceCredential;
            readonly int _errorHwUnavailable;
            readonly int _errorHwNotPresent;
            readonly int _errorLockout;
            readonly int _errorLockoutPermanent;
            public AuthenticationCallback(Action onSuccess, Action<BiometricFailureReason> onFailure, System.Threading.SynchronizationContext thread,
                int errorCanceled, int errorUserCanceled, int errorNegativeButton, int errorNoBiometrics,
                int errorNoDeviceCredential, int errorHwUnavailable, int errorHwNotPresent, int errorLockout, int errorLockoutPermanent)
                : base("com.ishix.nativebiometricauth.NativeBiometricAuthCallback")
            {
                _onSuccess = onSuccess;
                _onFailure = onFailure;
                _thread = thread;
                _errorCanceled = errorCanceled;
                _errorUserCanceled = errorUserCanceled;
                _errorNegativeButton = errorNegativeButton;
                _errorNoBiometrics = errorNoBiometrics;
                _errorNoDeviceCredential = errorNoDeviceCredential;
                _errorHwUnavailable = errorHwUnavailable;
                _errorHwNotPresent = errorHwNotPresent;
                _errorLockout = errorLockout;
                _errorLockoutPermanent = errorLockoutPermanent;
            }
            public void onAuthenticationSucceeded(AndroidJavaObject result) => _thread.Post(_ => _onSuccess?.Invoke(), null);
            public void onAuthenticationFailed() => _thread.Post(_ => _onFailure?.Invoke(BiometricFailureReason.AuthenticationFailed), null);
            public void onAuthenticationError(int errorCode, string errorString)
            {
                Debug.LogError("Biometric authentication error: " + errorString);
                var reason = MapErrorCode(errorCode);
                _thread.Post(_ => _onFailure?.Invoke(reason), null);
            }

            BiometricFailureReason MapErrorCode(int errorCode)
            {
                if (errorCode == _errorCanceled
                    || errorCode == _errorUserCanceled
                    || errorCode == _errorNegativeButton)
                {
                    return BiometricFailureReason.Canceled;
                }
                if (errorCode == _errorNoBiometrics
                    || errorCode == _errorNoDeviceCredential)
                {
                    return BiometricFailureReason.NotConfigured;
                }
                if (errorCode == _errorHwUnavailable
                    || errorCode == _errorHwNotPresent)
                {
                    return BiometricFailureReason.NotSupported;
                }
                if (errorCode == _errorLockout
                    || errorCode == _errorLockoutPermanent)
                {
                    return BiometricFailureReason.AuthenticationFailed;
                }
                return BiometricFailureReason.SystemError;
            }
        }

        static BiometricFailureReason MapAuthenticateError(int status, int noneEnrolled, int noHardware, int hwUnavailable)
        {
            if (status == noneEnrolled)
            {
                return BiometricFailureReason.NotConfigured;
            }
            if (status == noHardware || status == hwUnavailable)
            {
                return BiometricFailureReason.NotSupported;
            }
            return BiometricFailureReason.SystemError;
        }
    }
}
#endif
