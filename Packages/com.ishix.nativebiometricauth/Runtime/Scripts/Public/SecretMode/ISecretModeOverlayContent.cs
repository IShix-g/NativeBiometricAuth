
namespace NativeBiometricAuth
{
    /// <summary>
    /// シークレットモードのオーバーレイプレハブにアタッチされたコンポーネントが
    /// 認証結果を受け取るためのインターフェース。
    /// </summary>
    public interface ISecretModeOverlay
    {
        void OnSuccess();
        void OnFailure(BiometricFailureReason reason);
    }
}
