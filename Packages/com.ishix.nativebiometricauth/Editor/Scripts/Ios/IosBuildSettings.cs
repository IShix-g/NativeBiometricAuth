
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NativeBiometricAuth.Editor
{
    internal class IosBuildSettings : ScriptableObject
    {
        const string _settingsPath = "Assets/Editor/NativeBiometricAuth_IosBuildSettings.asset";
        
        [SerializeField] IosBuildConfig _iosBuildConfig;

        public static IosBuildSettings LoadOrCreate() 
            => AssetDatabase.LoadAssetAtPath<IosBuildSettings>(_settingsPath) is var settings && settings != null ? settings : Create();
        
        public static IosBuildSettings Create()
        {
            var settings = CreateInstance<IosBuildSettings>();
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory)
                && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            AssetDatabase.CreateAsset(settings, _settingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }
        
        public static IosBuildConfig GetIosConfig()
        {
            var asset = AssetDatabase.LoadAssetAtPath<IosBuildSettings>(_settingsPath);
            return asset != null ? asset._iosBuildConfig : IosBuildConfig.CreateDefault();
        }
    }
}
