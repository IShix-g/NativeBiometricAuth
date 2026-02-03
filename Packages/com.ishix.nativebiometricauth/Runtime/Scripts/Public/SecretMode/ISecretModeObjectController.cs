
namespace NativeBiometricAuth
{
    public interface ISecretModeObjectController
    {
        void Show();
        void Hide();
        void OnSuccess();
        void OnFailure(BiometricFailureReason reason);
    }
}