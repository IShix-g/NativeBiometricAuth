
namespace NativeBiometricAuth
{
    public readonly struct BiometricAvailabilityStatus
    {
        public BiometricAvailabilityStatus(BiometricAvailability biometrics, BiometricAvailability deviceCredential)
        {
            Biometrics = biometrics;
            DeviceCredential = deviceCredential;
        }
        
        public BiometricAvailability Biometrics { get; }
        public BiometricAvailability DeviceCredential { get; }
    }
}