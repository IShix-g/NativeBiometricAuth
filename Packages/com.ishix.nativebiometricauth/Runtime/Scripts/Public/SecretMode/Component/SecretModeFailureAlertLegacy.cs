
using UnityEngine;
using UnityEngine.UI;

namespace NativeBiometricAuth
{
    [AddComponentMenu("Native Biometric Auth/Secret Mode Failure Alert (Legacy)")]
    public sealed class SecretModeFailureAlertLegacy : SecretModeFailureAlertBase
    {
        [SerializeField] Button _button;
        [SerializeField] Text _text;

        protected override void Start()
        {
            base.Start();
            _button.onClick.AddListener(ManualHide);
        }
        
        protected override void SetMessage(string message)
            => _text.text = message;
    }
}