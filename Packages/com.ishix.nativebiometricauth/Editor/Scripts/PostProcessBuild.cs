
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
            var plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            var rootDict = plist.root;
            rootDict.SetString("NSFaceIDUsageDescription", "This app uses Face ID to unlock features securely.");
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
