
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SampleExporter
{
    const string _sourcePath = "Assets/Samples/SecretModeTest";
    const string _destPath = "Packages/com.ishix.nativebiometricauth/Samples~/SecretModeTest";
    const string _asmDefName = "NativeBiometricAuth.SecretModeTest";
    const string _packageAsmDefPath = "Packages/com.ishix.nativebiometricauth/Runtime/Scripts/NativeBiometricAuth.asmdef";

    [MenuItem("Tools/Export Sample/SecretModeTest")]
    public static void Export()
    {
        if (Directory.Exists(_destPath))
        {
            Directory.Delete(_destPath, true);
        }
        FileUtil.CopyFileOrDirectory(_sourcePath, _destPath);
        GenerateAsmDef(_destPath);
        AssetDatabase.Refresh();
        Debug.Log("Sample exported to " + _destPath);
    }
    
    static void GenerateAsmDef(string rootPath)
    {
        var asmDefGuid = AssetDatabase.AssetPathToGUID(_packageAsmDefPath);
        var asmdefPath = Path.Combine(rootPath, $"{_asmDefName}.asmdef");
        var asmdefContent = $@"{{
    ""name"": ""{_asmDefName}"",
    ""rootNamespace"": """",
    ""references"": [
        ""GUID:{asmDefGuid}""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": false,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}}";
        File.WriteAllText(asmdefPath, asmdefContent);
    }
}