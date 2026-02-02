
using UnityEditor;
using UnityEngine;

namespace NativeBiometricAuth.Editor
{
    public sealed class IosSettingsWindow : EditorWindow
    {
        IosBuildSettings _settings;
        SerializedObject _serializedSettings;

        [MenuItem("Window/Native Biometric Auth/iOS Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<IosSettingsWindow>("Native Biometric Auth");
            window.minSize = new Vector2(300, 200);
        }

        void OnEnable()
        {
            _settings = IosBuildSettings.LoadOrCreate();
            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }
        }

        void OnGUI()
        {
            if (_serializedSettings == null)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("iOS Biometric Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("These settings will be applied to the Xcode project.", MessageType.Info);
            EditorGUILayout.Space();

            _serializedSettings.Update();
            
            var iterator = _serializedSettings.GetIterator();
            var enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name == "m_Script")
                {
                    continue;
                }
                EditorGUILayout.PropertyField(iterator, true);
            }
            if (_serializedSettings.ApplyModifiedProperties())
            {
                AssetDatabase.SaveAssets();
            }
        }
    }
}