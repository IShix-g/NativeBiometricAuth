
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace NativeBiometricAuth
{
    [AddComponentMenu("Native Biometric Auth/Secret Mode Button (Legacy)")]
    internal sealed class SecretModeButtonLegacy : SecretModeButtonBase
    {
        [SerializeField] Text _text;
        [SerializeField] string[] _strings = new []{ "Secret Mode: Activate", "Secret Mode: Deactivate"};

        void Awake()
             => Assert.IsTrue(_strings.Length == 2, "Two strings are required");
        
        protected override void OnUpdateText(bool isActive)
            => _text.text = _strings[isActive ? 0 : 1];

        protected override void Reset()
        {
            base.Reset();
            _text = GetComponentInChildren<Text>();
        }
    }
}