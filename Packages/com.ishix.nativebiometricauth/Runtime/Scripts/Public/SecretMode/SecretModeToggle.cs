
using UnityEngine;
using UnityEngine.UI;

namespace NativeBiometricAuth
{
    [AddComponentMenu("Native Biometric Auth/Secret Mode Toggle")]
    [RequireComponent(typeof(Toggle))]
    internal sealed class SecretModeToggle : SecretModeObserver
    {
        [SerializeField] Toggle _toggle;

        protected override void OnSecretModeInitialized(bool isActive)
        {
            _toggle.isOn = isActive;
            _toggle.onValueChanged.AddListener(RequestSecretModeActive);
        }

        protected override void OnSecretModeActiveChanged(bool isActive)
            => _toggle.SetIsOnWithoutNotify(isActive);

        protected override void OnAuthenticationFailure(BiometricFailureReason reason)
            => _toggle.SetIsOnWithoutNotify(IsActive);

        protected override void OnRelease() {}
        
        void Reset() => _toggle = GetComponent<Toggle>();
    }
}