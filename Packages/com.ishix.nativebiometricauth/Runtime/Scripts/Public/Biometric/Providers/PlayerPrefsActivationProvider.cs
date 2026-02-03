
using UnityEngine;

namespace NativeBiometricAuth
{
    sealed class PlayerPrefsActivationProvider : IBiometricActivationProvider
    {
        const string _key = "NativeBiometricAuth.Biometric.IsActive";

        public bool TryGet(out bool isActive)
        {
            if (!PlayerPrefs.HasKey(_key))
            {
                isActive = false;
                return false;
            }
            isActive = PlayerPrefs.GetInt(_key, 1) == 1;
            return true;
        }

        public void Set(bool isActive)
        {
            PlayerPrefs.SetInt(_key, isActive ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}