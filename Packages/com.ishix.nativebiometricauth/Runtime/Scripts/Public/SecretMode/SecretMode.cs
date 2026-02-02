
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
        internal static ISecretModeObjectController ObjectController => s_objectController;
        internal static bool AllowDeviceCredential { get; private set; }
        internal static float ResumeGraceSeconds { get; private set; }
        
        static SecretModeController s_controller;
        static ISecretModeObjectController s_objectController;
        static bool s_skipOverlayOnActivate;

        public static void Initialize(
            GameObject overlayPrefab,
            bool allowDeviceCredential = true,
            float resumeGraceSeconds = 10f)
            => InitializeInternal(overlayPrefab, null, allowDeviceCredential, resumeGraceSeconds);

        public static void Initialize(
            ISecretModeObjectController objectController,
            bool allowDeviceCredential = true,
            float resumeGraceSeconds = 10f)
            => InitializeInternal(null, objectController, allowDeviceCredential, resumeGraceSeconds);

        static void InitializeInternal(
            GameObject overlayPrefab,
            ISecretModeObjectController objectController,
            bool allowDeviceCredential,
            float resumeGraceSeconds)
        {
            if (overlayPrefab == null
                && objectController == null)
            {
                Debug.LogError("[NativeBiometricAuth] SecretMode.Initialize requires a prefab or ISecretModeObjectController.");
                return;
            }

            if (s_objectController is IDisposable disposable)
            {
                disposable.Dispose();
            }
            s_objectController = objectController ?? new PrefabSecretModeObjectController(overlayPrefab);
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
            Biometric.SetActive(isActive, true, () =>
            {
                NotifyAuthenticateSuccess();
                onSuccess?.Invoke();
            }, reason =>
            {
                NotifyAuthenticateFailure(reason);
                onFailure?.Invoke(reason);
            });
        }

        internal static void NotifyAuthenticateSuccess()
        {
            s_objectController.OnSuccess();
            OnAuthenticateSuccess();
        }

        internal static void NotifyAuthenticateFailure(BiometricFailureReason reason)
        {
            s_objectController.OnFailure(reason);
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
