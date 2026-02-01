
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace NativeBiometricAuth
{
    internal sealed class TestOverlayCanvas : MonoBehaviour, ISecretModeOverlay
    {
        static readonly Regex s_pascalCaseRegex = new ("([a-z])([A-Z])", RegexOptions.Singleline);
        
        [SerializeField] Text _errorText;
        
        void Awake() => _errorText.gameObject.SetActive(false);
        
        public void OnSuccess() => _errorText.gameObject.SetActive(false);
        
        public void OnFailure(BiometricFailureReason reason)
        {
            _errorText.gameObject.SetActive(true);
            _errorText.text = "Failed: " + SplitPascalCase(reason.ToString());
        }
        
        static string SplitPascalCase(string input) => s_pascalCaseRegex.Replace(input, "$1 $2");
    }
}