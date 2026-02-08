
using UnityEditor;
using UnityEngine;

namespace NativeBiometricAuth.Editor
{
    internal sealed class BiometricSettingsWindow : EditorWindow
    {
        BiometricSettings _settings;
        SerializedObject _serializedSettings;

        [MenuItem("Window/Native Biometric Auth/Error Message Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<BiometricSettingsWindow>("Native Biometric Auth");
            window.minSize = new Vector2(300, 200);
        }

        void OnEnable()
        {
            _settings = BiometricSettings.Instance;
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
            EditorGUILayout.LabelField("Error Message Settings", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Please set the messages to be displayed when an error occurs.", MessageType.Info);
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