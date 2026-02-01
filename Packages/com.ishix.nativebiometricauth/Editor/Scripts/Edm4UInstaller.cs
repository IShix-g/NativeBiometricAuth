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
        const string _menuItemPath = "Window/Native Biometric Auth/Install Google External Dependency Manager";
        const string _updateMenuItemPath = "Window/Native Biometric Auth/Update Google External Dependency Manager";
        const string _guideMenuItemPath = "Window/Native Biometric Auth/Android Setup Guide";
        const string _dialogTitle = "Native Biometric Auth";

        static bool s_isCheckedEdmVersion;
        static bool s_isNeedUpdateEdmVersion;
        static CancellationTokenSource s_tokenSource;
        static readonly PackageInstaller s_packageInstaller = new();

#if !ENABLE_EDM4U
        [MenuItem(_menuItemPath)]
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
                .Handled(() =>
                {
                    Debug.Log("[NativeBiometricAuth] EDM4U Installation Completed.");
                    ShowSetupInstructions();
                });
        }

        public static void PromptInstallIfNeeded()
        {
            if (IsInstalled()) return;

            var result = EditorUtility.DisplayDialog(
                _dialogTitle,
                "This plugin requires 'Google External Dependency Manager' (EDM4U) to work properly.\n\nDo you want to install it now?",
                "Install",
                "Not Now"
            );

            if (result) Install();
        }

        public static void UpdateIfNeeded()
        {
            if (!IsInstalled()) return;

            CheckVersionInternal();
            if (s_isNeedUpdateEdmVersion) ShowUpdateDialog();
        }

        public static void CheckForUpdate()
        {
            if (!IsInstalled()) return;

            s_isCheckedEdmVersion = false;
            CheckVersionInternal();

            if (s_isNeedUpdateEdmVersion)
            {
                ShowUpdateDialog();
            }
            else
            {
                EditorUtility.DisplayDialog(_dialogTitle, "Google External Dependency Manager is up to date.", "OK");
            }
        }

        static void CheckVersionInternal()
        {
            if (s_isCheckedEdmVersion) return;

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
                    .ContinueOnMainThread(onSuccess: task => s_isNeedUpdateEdmVersion = false);
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
            if (string.IsNullOrEmpty(current) || string.IsNullOrEmpty(request)) return false;
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