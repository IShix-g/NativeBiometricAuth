
using UnityEditor;

namespace NativeBiometricAuth.Editor
{
    internal sealed class Bootstrapper
    {
        const string _key = "NativeBiometricAuth_Bootstrapper_IsInitialized";

        static bool IsInitialized
        {
            get => SessionState.GetBool(_key, false);
            set => SessionState.SetBool(_key, value);
        }

        [InitializeOnLoadMethod]
        static void OnDomainReload()
        {
            if (IsInitialized)
            {
                return;
            }
            IsInitialized = true;
            EditorApplication.delayCall += LoadSettings;
        }

        static void LoadSettings()
        {
#if !ENABLE_EDM4U
            Edm4UInstaller.PromptInstallIfNeeded();
#else
            Edm4UInstaller.UpdateIfNeeded();
#endif
        }
    }
}