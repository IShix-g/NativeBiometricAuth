package com.ishix.nativebiometricauth;
import androidx.biometric.BiometricPrompt;
public class NativeBiometricPromptCallback extends BiometricPrompt.AuthenticationCallback {
    private NativeBiometricAuthCallback biometricAuthCallback;
    public NativeBiometricPromptCallback(NativeBiometricAuthCallback biometricAuthCallback) {
        this.biometricAuthCallback = biometricAuthCallback;
    }
    @Override
    public void onAuthenticationError(int errorCode, CharSequence errString) {
        biometricAuthCallback.onAuthenticationError(errorCode, errString);
        NativeBiometricAuthActivity.finishAndClose();
    }
    @Override
    public void onAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result) {
        biometricAuthCallback.onAuthenticationSucceeded(result);
        NativeBiometricAuthActivity.finishAndClose();
    }
    @Override
    public void onAuthenticationFailed() {
        biometricAuthCallback.onAuthenticationFailed();
        NativeBiometricAuthActivity.finishAndClose();
    }
}
