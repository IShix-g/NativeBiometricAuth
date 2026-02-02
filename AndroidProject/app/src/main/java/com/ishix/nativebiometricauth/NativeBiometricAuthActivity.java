package com.ishix.nativebiometricauth;
import androidx.fragment.app.FragmentActivity;
import android.os.Bundle;
import android.content.Intent;
public class NativeBiometricAuthActivity extends FragmentActivity {
    private static OnHostReadyCallback onHostReadyCallback;
    private static NativeBiometricAuthActivity instance;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        instance = this;
        if (onHostReadyCallback != null) {
            onHostReadyCallback.onHostReady(this);
        }
    }
    public static void launchFromUnity(OnHostReadyCallback callback) {
        onHostReadyCallback = callback;
        Intent intent = new Intent(com.unity3d.player.UnityPlayer.currentActivity, NativeBiometricAuthActivity.class);
        com.unity3d.player.UnityPlayer.currentActivity.startActivity(intent);
    }
    public static void finishAndClose() {
        if (instance != null) {
            instance.finish();
        }
    }
    public interface OnHostReadyCallback {
        public void onHostReady(FragmentActivity activity);
    }
}
