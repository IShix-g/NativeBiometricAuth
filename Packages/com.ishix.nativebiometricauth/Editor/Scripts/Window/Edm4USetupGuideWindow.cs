
using UnityEngine;
using UnityEditor;

namespace NativeBiometricAuth.Editor
{
    internal sealed class Edm4USetupGuideWindow : EditorWindow
    {
        const string _imagePath1 = "Packages/com.ishix.nativebiometricauth/Editor/Textures/edm4u.jpg";
        const string _imagePath2 = "Packages/com.ishix.nativebiometricauth/Editor/Textures/edm4u2.jpg";
        const string _imagePath3 = "Packages/com.ishix.nativebiometricauth/Editor/Textures/edm4u3.jpg";
        const string _imagePath4 = "Packages/com.ishix.nativebiometricauth/Editor/Textures/edm4u4.jpg";
        
        Vector2 _scrollPosition;

        public static void ShowWindow()
        {
            var window = GetWindow<Edm4USetupGuideWindow>("Android Setup Guide");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        void OnGUI()
        {
            using var scroll = new EditorGUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scroll.scrollPosition;

            EditorGUILayout.BeginVertical(new GUIStyle() {padding = new RectOffset(10, 10, 10, 10)});

            {
                var style = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 10, 10),
                    fontSize = 18,
                };
                GUILayout.Label("Android Setup Instructions", style);
                EditorGUILayout.Space();
            }


            DrawStep("1. Switch Platform", "Switch your build target to 'Android' in Build Settings.");
            DrawStep("2. Enable Auto-resolution", "Select 'Enable' when this dialog appears.", _imagePath1);
            DrawStep("3. Stop Resolving", "If resolution starts automatically, please stop it. If it cannot be stopped, please wait for it to complete.", _imagePath4);
            DrawStep("4. Configure Publishing Settings", "Enable the following Gradle templates in Project Settings > Player - Android -> Publishing Settings -> Build.", _imagePath2);
            DrawStep("5. Force Resolve", "Run 'Assets > External Dependency Manager > Android Resolver > Force Resolve' to apply changes.", _imagePath3);

            EditorGUILayout.Space();
            if (GUILayout.Button("Open Player Settings", GUILayout.Height(30)))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }
            
            EditorGUILayout.EndVertical();
        }

        void DrawStep(string title, string description, string imagePath = null)
        {
            {
                var style = new GUIStyle(EditorStyles.toolbarButton)
                {
                    fontSize = 16,
                    fixedHeight = 26,
                };
                EditorGUILayout.LabelField(title, style);
            }

            EditorGUI.indentLevel++;

            {
                var style = new GUIStyle(EditorStyles.wordWrappedLabel)
                {
                    padding = new RectOffset(0, 0, 10, 10),
                    fontSize = 14,
                };
                EditorGUILayout.LabelField(description, style);
            }

            
            if (!string.IsNullOrEmpty(imagePath))
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
                if (tex != null)
                {
                    var aspect = (float)tex.height / tex.width;
                    var width = position.width - 40;
                    var rect = GUILayoutUtility.GetRect(width, width * aspect);
                    GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit);
                    EditorGUILayout.Space(10);
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
    }
}