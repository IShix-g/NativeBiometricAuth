#if ENABLE_TEXT_MESH_PRO
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

namespace NativeBiometricAuth
{
    [AddComponentMenu("Native Biometric Auth/Secret Mode Button")]
    [RequireComponent(typeof(Text))]
    internal sealed class SecretModeButton : SecretModeButtonBase
    {
        [SerializeField] TextMeshProUGUI _text;
        [SerializeField] string[] _strings = new []{ "Secret Mode: Activate", "Secret Mode: Deactivate"};

        void Awake()
            => Assert.IsTrue(_strings.Length == 2, "Two strings are required");
        
        protected override void OnUpdateText(bool isActive)
            => _text.text = _strings[isActive ? 0 : 1];

        protected override void Reset()
        {
            base.Reset();
            _text = GetComponent<TextMeshProUGUI>();
        }
    }
}
#endif