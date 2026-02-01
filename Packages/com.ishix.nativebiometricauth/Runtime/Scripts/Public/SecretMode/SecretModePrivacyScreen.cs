
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NativeBiometricAuth
{
    internal static class SecretModePrivacyScreen
    {
        static bool s_enabled;
        static bool s_pendingAndroidApply;
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        static bool s_macNativeAvailable = true;
#endif

        internal static void SetEnabled(bool enabled)
        {
            if (s_enabled == enabled)
            {
                return;
            }
            s_enabled = enabled;
#if UNITY_IOS && !UNITY_EDITOR
            NBP_SetPrivacyScreenEnabled(enabled);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            if (s_macNativeAvailable)
            {
                try
                {
                    NBP_SetPrivacyScreenEnabled(enabled);
                }
                catch (DllNotFoundException)
                {
                    s_macNativeAvailable = false;
                    Debug.LogError("Privacy screen native plugin not found on macOS.");
                }
                catch (EntryPointNotFoundException)
                {
                    s_macNativeAvailable = false;
                    Debug.LogError("Privacy screen entry point not found on macOS.");
                }
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (s_pendingAndroidApply)
            {
                return;
            }
            s_pendingAndroidApply = true;
            ApplyAndroidSecureFlag(enabled);
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void NBP_SetPrivacyScreenEnabled([MarshalAs(UnmanagedType.I1)] bool enabled);
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
        [DllImport("SecretModePrivacyScreen")]
        static extern void NBP_SetPrivacyScreenEnabled([MarshalAs(UnmanagedType.I1)] bool enabled);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        static void ApplyAndroidSecureFlag(bool enabled)
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (activity == null)
                    {
                        s_pendingAndroidApply = false;
                        Debug.LogError("Failed to update FLAG_SECURE: currentActivity is null.");
                        return;
                    }
                    const int flagSecure = 8192;
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        using (activity)
                        {
                            var window = activity.Call<AndroidJavaObject>("getWindow");
                            if (window == null)
                            {
                                Debug.LogError("Failed to update FLAG_SECURE: window is null.");
                                s_pendingAndroidApply = false;
                                return;
                            }
                            using (window)
                            {
                                if (enabled)
                                {
                                    window.Call("addFlags", flagSecure);
                                }
                                else
                                {
                                    window.Call("clearFlags", flagSecure);
                                }
                            }
                        }
                        s_pendingAndroidApply = false;
                    }));
                }
            }
            catch (Exception e)
            {
                s_pendingAndroidApply = false;
                Debug.LogError($"Failed to update FLAG_SECURE: {e.Message}");
            }
        }
#endif
    }
}
