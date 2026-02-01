
using UnityEngine;
using NativeBiometricAuth;

namespace RunTimeTest
{
    internal sealed class SecretModeTest : MonoBehaviour
    {
        [SerializeField] GameObject _overlayPrefab;
        
        void Awake()
        {
            if (!SecretMode.IsInitialized)
            {
                SecretMode.Initialize(
                    overlayPrefab: _overlayPrefab,
                    allowDeviceCredential: true,
                    resumeGraceSeconds: 10
                );
            }
        }

        void Start()
        {
            SecretMode.OnSecretModeActiveChanged += OnSecretModeActiveChanged;
            SecretMode.OnAuthenticateSuccess += OnAuthenticateSuccess;
            SecretMode.OnAuthenticateFailure += OnAuthenticateFailure;
        }
        
        void OnDestroy()
        {
            SecretMode.OnSecretModeActiveChanged -= OnSecretModeActiveChanged;
            SecretMode.OnAuthenticateSuccess -= OnAuthenticateSuccess;
            SecretMode.OnAuthenticateFailure -= OnAuthenticateFailure;
        }
        
        void OnSecretModeActiveChanged(bool isActive)
            => Debug.Log("OnSecretModeActiveChanged: " + isActive);
        
        void OnAuthenticateSuccess()
            => Debug.Log("OnAuthenticateSuccess");
        
        void OnAuthenticateFailure(BiometricFailureReason reason)
        {
            Debug.Log("OnAuthenticateFailure: " + reason);
        }
    }
}