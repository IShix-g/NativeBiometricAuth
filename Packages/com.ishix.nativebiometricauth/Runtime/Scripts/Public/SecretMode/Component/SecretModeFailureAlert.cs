#if ENABLE_TEXT_MESH_PRO
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NativeBiometricAuth
{
    [AddComponentMenu("Native Biometric Auth/Secret Mode Failure Alert")]
    public sealed class SecretModeFailureAlert : SecretModeFailureAlertBase
    {
        [SerializeField] Button _button;
        [SerializeField] TextMeshProUGUI _text;
        
        protected override void Start()
        {
            base.Start();
            _button.onClick.AddListener(ManualHide);
        }
        
        protected override void SetMessage(string message)
            => _text.text = message;
    }
}
#endif