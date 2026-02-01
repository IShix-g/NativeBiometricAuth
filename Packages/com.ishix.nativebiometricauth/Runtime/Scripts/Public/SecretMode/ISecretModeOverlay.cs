
namespace NativeBiometricAuth
{
    public interface ISecretModeOverlayController
    {
        void Show();
        void Hide();
        void OnSuccess();
        void OnFailure(BiometricFailureReason reason);
    }
}