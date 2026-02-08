
using UnityEditor;
using UnityEngine;

namespace NativeBiometricAuth.Editor
{
    internal static class PackageResources
    {
        [MenuItem("Window/Native Biometric Auth/GitHub Repository")]
        public static void OpenGitHubRepository()
            => Application.OpenURL("https://github.com/IShix-g/NativeBiometricAuth");
    }
}