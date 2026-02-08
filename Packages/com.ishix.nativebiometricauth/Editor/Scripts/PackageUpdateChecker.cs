
using System;
using System.Threading.Tasks;
using UnityEditor;

namespace NativeBiometricAuth.Editor
{
    internal static class PackageUpdateChecker
    {
        const string _gitInstallUrl = "https://github.com/IShix-g/NativeBiometricAuth.git?path=Packages/com.ishix.nativebiometricauth";
        const string _gitBranchName = "main";
        const string _packageName = "com.ishix.nativebiometricauth";
        
        [MenuItem("Window/Native Biometric Auth/Check for Update")]
        public static void CheckForUpdate()
        {
            EditorUtility.DisplayProgressBar("Native Biometric Auth", "Checking for updates...", 0.5f);
            
            var versionChecker = new PackageVersionChecker(_gitInstallUrl, _gitBranchName, _packageName);
            versionChecker.Fetch()
                .Handled(() =>
                {
                    EditorUtility.ClearProgressBar();
                    
                    if (versionChecker.HasNewVersion())
                    {
                        ShowUpdateDialog(versionChecker).Handled();
                    }
                    else
                    {
                        ShowUpToDateDialog(versionChecker.LocalInfo.version);
                        versionChecker.Dispose();
                    }
                });
        }

        static async Task ShowUpdateDialog(PackageVersionChecker checker)
        {
            var newVersion = checker.ServerInfo.version;
            var message = $"A new version ({newVersion}) is available. Would you like to update?";
            if (EditorUtility.DisplayDialog("Update Available", message, "Update", "Not Now"))
            {
                try
                {
                    await checker.InstallAsync();
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
                finally
                {
                    checker.Dispose();
                }
            }
        }
        
        static void ShowUpToDateDialog(string currentVersion)
        {
            var message = $"You are using the latest version: {currentVersion}";
            EditorUtility.DisplayDialog("Check for Update", message, "OK");
        }
    }
}