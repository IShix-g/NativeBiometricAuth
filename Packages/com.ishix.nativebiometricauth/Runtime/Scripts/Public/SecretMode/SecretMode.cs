
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NativeBiometricAuth
{
    public static class SecretMode
    {
        public static event Action OnInitialized = delegate { };
        public static event Action<bool> OnSecretModeActiveChanged = delegate { };
        public static event Action OnAuthenticateSuccess = delegate { };
        public static event Action<BiometricFailureReason> OnAuthenticateFailure = delegate { };
        
        public static bool IsInitialized { get; private set; }
        public static bool IsActive => Biometric.IsActive;
        public static bool IsAvailable => Biometric.IsAvailable(AllowDeviceCredential);
        internal static ISecretModeOverlayController OverlayController => s_overlayController;
        internal static bool AllowDeviceCredential { get; private set; }
        internal static float ResumeGraceSeconds { get; private set; }
        
        static SecretModeController s_controller;
        static ISecretModeOverlayController s_overlayController;
        static bool s_skipOverlayOnActivate;

        public static void Initialize(
            GameObject overlayPrefab,
            bool allowDeviceCredential = true,
            float resumeGraceSeconds = 10f)
            => InitializeInternal(overlayPrefab, null, allowDeviceCredential, resumeGraceSeconds);

        public static void Initialize(
            ISecretModeOverlayController overlayController,
            bool allowDeviceCredential = true,
            float resumeGraceSeconds = 10f)
            => InitializeInternal(null, overlayController, allowDeviceCredential, resumeGraceSeconds);

        static void InitializeInternal(
            GameObject overlayPrefab,
            ISecretModeOverlayController overlayController,
            bool allowDeviceCredential,
            float resumeGraceSeconds)
        {
            if (overlayPrefab == null
                && overlayController == null)
            {
                Debug.LogError("[NativeBiometricAuth] SecretMode.Initialize requires a prefab or overlayFactory.");
                return;
            }

            if (s_overlayController is IDisposable disposable)
            {
                disposable.Dispose();
            }
            s_overlayController = overlayController ?? new PrefabSecretModeOverlayController(overlayPrefab);
            AllowDeviceCredential = allowDeviceCredential;
            ResumeGraceSeconds = Mathf.Max(0f, resumeGraceSeconds);

            if (s_controller == null)
            {
                var host = new GameObject("NativeBiometricAuth-SecretMode");
                Object.DontDestroyOnLoad(host);
                s_controller = host.AddComponent<SecretModeController>();
                s_controller.ShowAndAuthenticate();
            }
            IsInitialized = true;
            OnInitialized();
        }

        public static void SetActive(bool isActive, Action onSuccess = null, Action<BiometricFailureReason> onFailure = null)
        {
            if (isActive)
            {
                s_skipOverlayOnActivate = true;
            }
            Biometric.SetActive(isActive, true, onSuccess, onFailure);
        }

        internal static void NotifyAuthenticateSuccess()
        {
            s_overlayController.OnSuccess();
            OnAuthenticateSuccess();
        }

        internal static void NotifyAuthenticateFailure(BiometricFailureReason reason)
        {
            s_overlayController.OnFailure(reason);
            OnAuthenticateFailure(reason);
        }
        
        internal static bool ShouldUseBiometric()
            => Biometric.IsActive
               && Biometric.IsAvailable(AllowDeviceCredential);

        internal static void NotifySecretModeActiveChanged(bool isActive)
            => OnSecretModeActiveChanged(isActive);

        internal static bool ConsumeSkipOverlayOnActivate()
        {
            if (!s_skipOverlayOnActivate)
            {
                return false;
            }
            s_skipOverlayOnActivate = false;
            return true;
        }
    }
}
