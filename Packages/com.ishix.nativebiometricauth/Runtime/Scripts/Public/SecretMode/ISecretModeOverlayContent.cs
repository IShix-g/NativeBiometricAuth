
namespace NativeBiometricAuth
{
    public interface ISecretModeObject
    {
        void Show();
        void Hide();
        void OnSuccess();
        void OnFailure(BiometricFailureReason reason);
    }
}
