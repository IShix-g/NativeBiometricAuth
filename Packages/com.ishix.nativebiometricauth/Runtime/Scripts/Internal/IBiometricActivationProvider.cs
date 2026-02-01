
namespace NativeBiometricAuth
{
    public interface IBiometricActivationProvider
    {
        bool TryGet(out bool isActive);
        void Set(bool isActive);
    }
}