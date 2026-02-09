
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NativeBiometricAuth;

public class NativeBiometricAuthTest : MonoBehaviour
{
    [SerializeField] Image _blockImage;
    [SerializeField] Button _button;
    [SerializeField] Toggle _toggle;

    static bool IsOn
    {
        get => PlayerPrefs.GetInt("NativeBiometricAuthTest_IsOn", 0) == 1;
        set => PlayerPrefs.SetInt("NativeBiometricAuthTest_IsOn", value ? 1 : 0);
    }
    
    IEnumerator Start()
    {
        _blockImage.gameObject.SetActive(true);
        _toggle.isOn = IsOn;
        yield return null;
        if (!Biometric.IsActive)
        {
            Biometric.SetActive(true, _toggle.isOn);
        }
        else
        {
            Biometric.Authenticate(_toggle.isOn, OnSuccess, OnFailure);
        }
        _button.onClick.AddListener(ClickButton);
    }

    void OnDestroy() => IsOn = _toggle.isOn;
    
    void ClickButton() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    
    void OnSuccess()
    {
        _blockImage.gameObject.SetActive(false);
        Debug.Log("OnSuccess");
    }

    void OnFailure(BiometricFailureReason reason)
    {
        Debug.Log("OnFailure: " + reason);
        _blockImage.color = Color.red;
    }
}
