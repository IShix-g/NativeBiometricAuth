
using System;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace NativeBiometricAuth.Editor
{
    internal static class Edm4UInstaller
    {
        const string _gitInstallUrl = "https://github.com/googlesamples/unity-jar-resolver.git?path=upm#v1.2.187";
        const string _packagePath = "Packages/com.google.external-dependency-manager/";
        public const string InstallMenuItemPath = "Window/Native Biometric Auth/Android/Install Google External Dependency Manager";
        const string _updateMenuItemPath = "Window/Native Biometric Auth/Android/Update Google External Dependency Manager";
        const string _guideMenuItemPath = "Window/Native Biometric Auth/Android/Setup Guide";
        const string _dialogTitle = "Native Biometric Auth";

        static bool s_isCheckedEdmVersion;
        static bool s_isNeedUpdateEdmVersion;
        static CancellationTokenSource s_tokenSource;
        static readonly PackageInstaller s_packageInstaller = new();

#if !ENABLE_EDM4U
        [MenuItem(InstallMenuItemPath)]
        public static void InstallFromMenu() => Install();
#else
        [MenuItem(_updateMenuItemPath)]
        public static void UpdateFromMenu() => CheckForUpdate();
#endif

        [MenuItem(_guideMenuItemPath)]
        public static void ShowSetupInstructions() => Edm4USetupGuideWindow.ShowWindow();

        static void Install()
        {
            s_tokenSource = new CancellationTokenSource();

            Debug.Log("[NativeBiometricAuth] Start installing Google External Dependency Manager...");

            s_packageInstaller.Install(
                    new[] { _gitInstallUrl },
                    s_tokenSource.Token
                )
                .Handled(() => Debug.Log("[NativeBiometricAuth] Google External Dependency Manager Installation Completed."));
        }

        public static void PromptInstallIfNeeded()
        {
            if (IsInstalled())
            {
                return;
            }
            var result = EditorUtility.DisplayDialog(
                _dialogTitle,
                "This plugin requires 'Google External Dependency Manager' (EDM4U) to work properly.\n\nDo you want to install it now?",
                "Install",
                "Not Now"
            );
            if (result)
            {
                Install();
            }
            else
            {
                Debug.LogWarning($"[NativeBiometricAuth] Installation canceled. You can install it later via '{InstallMenuItemPath}'.");
            }
        }

        public static void UpdateIfNeeded()
        {
            if (!IsInstalled())
            {
                return;
            }

            CheckVersionInternal();
            if (s_isNeedUpdateEdmVersion) ShowUpdateDialog();
        }

        public static void CheckForUpdate()
        {
            if (!IsInstalled())
            {
                return;
            }

            s_isCheckedEdmVersion = false;
            CheckVersionInternal();

            if (s_isNeedUpdateEdmVersion)
            {
                ShowUpdateDialog();
            }
            else
            {
                var version = CheckVersion.GetVersionFromUrl(_gitInstallUrl);
                EditorUtility.DisplayDialog(_dialogTitle, version + "\nGoogle External Dependency Manager is up to date.", "OK");
            }
        }

        static void CheckVersionInternal()
        {
            if (s_isCheckedEdmVersion)
            {
                return;
            }
            s_isCheckedEdmVersion = true;
            var current = CheckVersion.GetCurrent(_packagePath);
            var request = CheckVersion.GetVersionFromUrl(_gitInstallUrl);
            s_isNeedUpdateEdmVersion = IsVersionUpdated(current, request);
        }

        static void ShowUpdateDialog()
        {
            var result = EditorUtility.DisplayDialog(
                _dialogTitle,
                "A new version of EDM4U is available.\n\nUpdate is recommended for compatibility.\nDo you want to update?",
                "Update",
                "Not Now"
            );
            if (result)
            {
                s_tokenSource = new CancellationTokenSource();
                s_packageInstaller.Install(new[] { _gitInstallUrl }, s_tokenSource.Token)
                    .ContinueOnMainThread(onSuccess: _ => s_isNeedUpdateEdmVersion = false);
            }
            else
            {
                Debug.LogWarning($"[NativeBiometricAuth] Update canceled. You can update manually via '{_updateMenuItemPath}'.");
            }
        }

        static bool IsInstalled()
        {
#if ENABLE_EDM4U
            return true;
#else
            return false;
#endif
        }

        static bool IsVersionUpdated(string current, string request)
        {
            if (string.IsNullOrEmpty(current)
                || string.IsNullOrEmpty(request))
            {
                return false;
            }
            try
            {
                return new Version(current.TrimStart('v')) < new Version(request.TrimStart('v'));
            }
            catch
            {
                return false;
            }
        }
    }
}