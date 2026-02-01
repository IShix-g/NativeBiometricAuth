
namespace NativeBiometricAuth
{
    /// <summary>
    /// Represents the reason for a biometric authentication failure.
    /// </summary>
    public enum BiometricFailureReason
    {
        /// <summary>
        /// Biometric feature is disabled in app settings.
        /// </summary>
        Inactive = 0,
        /// <summary>
        /// Device does not support biometrics or device credential.
        /// </summary>
        NotSupported = 1,
        /// <summary>
        /// Supported but not enrolled or not configured.
        /// </summary>
        NotConfigured = 2,
        /// <summary>
        /// User canceled, negative button, or system canceled.
        /// </summary>
        Canceled = 3,
        /// <summary>
        /// Authentication failed (including lockout).
        /// </summary>
        AuthenticationFailed = 4,
        /// <summary>
        /// Unknown or system error.
        /// </summary>
        SystemError = 5
    }
}
