
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.iOS.Xcode;

namespace NativeBiometricAuth.Editor
{
    internal static class PostProcessBuild
    {
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.iOS)
            {
                AddFrameworks(pathToBuiltProject);
                AddFaceIDUsageDescription(pathToBuiltProject);
                Debug.Log("[NativeBiometricAuth] NSFaceIDUsageDescription added to Info.plist");
            }
            else if(target == BuildTarget.Android)
            {
                CheckEdm4URequirement();
            }
        }

        static void AddFrameworks(string pathToBuiltProject)
        {
            var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            var mainTargetGuid = project.GetUnityMainTargetGuid();
            project.AddFrameworkToProject(mainTargetGuid, "LocalAuthentication.framework", false);
#if UNITY_2019_3_OR_NEWER
            var frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
            project.AddFrameworkToProject(frameworkTargetGuid, "LocalAuthentication.framework", false);
#endif
            project.WriteToFile(projectPath);
        }

        static void AddFaceIDUsageDescription(string pathToBuiltProject)
        {
            var config = IosBuildSettings.GetIosConfig();
            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            var rootDict = plist.root;
            rootDict.SetString("NSFaceIDUsageDescription", config.NsFaceIDUsageDescription);
            File.WriteAllText(plistPath, plist.WriteToString());
        }
        
        static void CheckEdm4URequirement()
        {
#if !ENABLE_EDM4U
            var errorMessage = "[NativeBiometricAuth] Build Failed: Google External Dependency Manager (EDM4U) is required for Android builds.\n" +
                               $"Please install it via '{Edm4UInstaller.InstallMenuItemPath}'.";
            Debug.LogError(errorMessage);
            throw new BuildPlayerWindow.BuildMethodException(errorMessage);
#else
            Debug.Log("[NativeBiometricAuth] EDM4U requirement check passed.");
#endif
        }
    }
}
