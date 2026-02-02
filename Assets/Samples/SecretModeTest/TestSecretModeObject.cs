
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace NativeBiometricAuth.Samples
{
    internal sealed class TestSecretModeObject : MonoBehaviour, ISecretModeObject
    {
        static readonly Regex s_pascalCaseRegex = new ("([a-z])([A-Z])", RegexOptions.Singleline);
        
        [SerializeField] RectTransform _lockOverlay;
        [SerializeField] Text _errorText;
        [SerializeField] SecretModeFailureAlertLegacy _failureAlert;

        void Awake()
        {
            _lockOverlay.gameObject.SetActive(false);
            _errorText.gameObject.SetActive(false);
        }

        public void Show()
        {
            _lockOverlay.gameObject.SetActive(true);
            _errorText.gameObject.SetActive(false);
        }

        public void Hide() => _lockOverlay.gameObject.SetActive(false);

        public void OnSuccess()
        {
            _errorText.gameObject.SetActive(false);
            _failureAlert.Hide();
        }
        
        public void OnFailure(BiometricFailureReason reason)
        {
            _errorText.gameObject.SetActive(true);
            var reasonString = SplitPascalCase(reason.ToString());
            _errorText.text = reasonString;
            if (reason != BiometricFailureReason.Canceled)
            {
                _failureAlert.Show(reason, reasonString);
            }
        }
        
        static string SplitPascalCase(string input) => s_pascalCaseRegex.Replace(input, "$1 $2");
    }
}