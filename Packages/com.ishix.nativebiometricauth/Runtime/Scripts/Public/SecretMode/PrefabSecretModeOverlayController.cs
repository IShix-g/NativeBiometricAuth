
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NativeBiometricAuth
{
    internal sealed class PrefabSecretModeOverlayController : ISecretModeOverlayController, IDisposable
    {
        bool _isDisposed;
        GameObject _object;
        ISecretModeOverlay _overlayContent;

        public PrefabSecretModeOverlayController(GameObject prefab)
        {
            if (string.IsNullOrEmpty(prefab.scene.name))
            {
                _object = CreateInternal(prefab);
            }
            else
            {
                _object = prefab;
            }
            _overlayContent = _object.GetComponent<ISecretModeOverlay>();
            Object.DontDestroyOnLoad(_object);
            prefab.gameObject.SetActive(false);
            Hide();
        }

        public void Show() => _object.SetActive(true);

        public void Hide() => _object.SetActive(false);

        public void OnSuccess()
        {
            if (_overlayContent != null)
            {
                _overlayContent.OnSuccess();
            }
        }

        public void OnFailure(BiometricFailureReason reason)
        {
            if (_overlayContent != null)
            {
                _overlayContent.OnFailure(reason);
            }
        }

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
            _overlayContent = null;
        }
    }
}