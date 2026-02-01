
using UnityEngine;

namespace NativeBiometricAuth
{
    public abstract class SecretModeObserver : MonoBehaviour
    {
        public bool IsActive => SecretMode.IsActive;
        
        protected abstract void OnSecretModeInitialized(bool isActive);
        protected abstract void OnRelease();
        protected abstract void OnSecretModeActiveChanged(bool isActive);
        protected abstract void OnAuthenticationFailure(BiometricFailureReason reason);
        
        void Initialize()
        {
            SecretMode.OnInitialized -= Initialize;
            SecretMode.OnSecretModeActiveChanged += OnActiveChangedInternal;
            OnSecretModeInitialized(SecretMode.IsActive);
        }

        void Start()
        {
            if (SecretMode.IsInitialized)
            {
                Initialize();
            }
            else
            {
                SecretMode.OnInitialized += Initialize;
            }
        }

        void OnDestroy()
        {
            SecretMode.OnInitialized -= Initialize;
            SecretMode.OnSecretModeActiveChanged -= OnActiveChangedInternal;
            OnRelease();
        }

        protected void RequestSecretModeActive(bool isActive)
        {
            if (SecretMode.IsActive == isActive)
            {
                return;
            }
            SecretMode.SetActive(
                isActive: isActive,
                onFailure: OnAuthenticationFailure);
        }

        void OnActiveChangedInternal(bool isActive) => OnSecretModeActiveChanged(isActive);
    }
}