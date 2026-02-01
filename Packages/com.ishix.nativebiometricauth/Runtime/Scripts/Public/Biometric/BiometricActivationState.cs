
namespace NativeBiometricAuth
{
    /// <summary>
    /// Represents the state of biometric authentication.
    /// </summary>
    public enum BiometricActivationState
    {
        /// <summary>
        /// Represents the state where biometric authentication is disabled.
        /// </summary>
        Disabled = 0,
        /// <summary>
        /// Represents the state where biometric authentication is enabled.
        /// </summary>
        Enabled = 1,
        /// <summary>
        /// Represents the state where biometric authentication is enabled but not configured.
        /// </summary>
        NotConfigured = 2
    }
}