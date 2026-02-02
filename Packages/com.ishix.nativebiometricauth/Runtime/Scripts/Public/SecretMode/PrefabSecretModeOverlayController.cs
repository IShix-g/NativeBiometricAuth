
using System;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace NativeBiometricAuth
{
    internal sealed class PrefabSecretModeObjectController : ISecretModeObjectController, IDisposable
    {
        bool _isDisposed;
        GameObject _object;
        ISecretModeObject _objectContent;

        public PrefabSecretModeObjectController(GameObject prefab)
        {
            _object = string.IsNullOrEmpty(prefab.scene.name)
                ? CreateInternal(prefab)
                : prefab;
            _objectContent = _object.GetComponent<ISecretModeObject>();
            Object.DontDestroyOnLoad(_object);
            Assert.IsNotNull(_objectContent, "ISecretModeObject component not found on the provided object/prefab.");
            Hide();
        }

        public void Show() => _objectContent.Show();

        public void Hide() => _objectContent.Hide();

        public void OnSuccess() => _objectContent.OnSuccess();

        public void OnFailure(BiometricFailureReason reason) => _objectContent.OnFailure(reason);

        GameObject CreateInternal(GameObject prefab) => Object.Instantiate(prefab);

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            Object.Destroy(_object);
            _object = null;
            _objectContent = null;
        }
    }
}