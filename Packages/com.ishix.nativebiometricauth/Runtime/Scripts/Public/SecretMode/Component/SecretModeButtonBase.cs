
using UnityEngine;
using UnityEngine.UI;

namespace NativeBiometricAuth
{
    [RequireComponent(typeof(Button))]
    internal abstract class SecretModeButtonBase : SecretModeObserver
    {
        [SerializeField] Button _button;
        
        protected abstract void OnUpdateText(bool isActive);
        
        protected override void OnSecretModeInitialized(bool isActive)
        {
            OnUpdateText(isActive);
            _button.onClick.AddListener(ClickButton);
        }

        void ClickButton() => RequestSecretModeActive(!IsActive);

        protected override void OnSecretModeActiveChanged(bool isActive)
            => OnUpdateText(isActive);

        protected override void OnAuthenticationFailure(BiometricFailureReason reason)
            => OnUpdateText(IsActive);

        protected override void OnRelease() {}
        
        protected virtual void Reset() => _button = GetComponent<Button>();
    }
}