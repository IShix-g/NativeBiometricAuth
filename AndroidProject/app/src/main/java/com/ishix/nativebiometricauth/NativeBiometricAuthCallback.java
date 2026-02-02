package com.ishix.nativebiometricauth;
import androidx.biometric.BiometricPrompt;
public interface NativeBiometricAuthCallback {
    public void onAuthenticationError(int errorCode, CharSequence errString);
    public void onAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result);
    public void onAuthenticationFailed();
}
