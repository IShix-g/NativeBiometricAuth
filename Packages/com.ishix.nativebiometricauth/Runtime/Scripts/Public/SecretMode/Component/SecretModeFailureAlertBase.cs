
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NativeBiometricAuth
{
    public abstract class SecretModeFailureAlertBase : MonoBehaviour
    {
        const int _hideMillisecondsDelay = 1500;
        const float _animationDuration = 0.3f;
        
        [SerializeField] RectTransform _parent;
        [SerializeField] CanvasGroup _group;
        [SerializeField] ValueAnimator _animator;

        CancellationTokenSource _cts;

        protected abstract void SetMessage(string message);
        
        protected virtual void Start()
        {
            _group.alpha = 0;
            _parent.gameObject.SetActive(false);
        }

        public void Show(BiometricFailureReason reason, string message)
        {
            CancelAndDisposeTokenIfNeeded();
            if (_animator.IsPlaying)
            {
                _animator.Stop();
            }

            SetMessage(message);
            _parent.gameObject.SetActive(true);
            _animator.Play(
                _group.alpha, 1f,
                _animationDuration,
                value => _group.alpha = value
            );

            _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _ = HideDelayAsync(_cts.Token);
        }
        
        protected void ManualHide()
        {
            CancelAndDisposeTokenIfNeeded();
            Hide();
        }
        
        public void Hide()
        {
            if (_animator.IsPlaying)
            {
                _animator.Stop();
            }
            _parent.gameObject.SetActive(true);
            _animator.Play(
                _group.alpha,
                0,
                _animationDuration,
                value => _group.alpha = value,
                () => _parent.gameObject.SetActive(false)
            );
        }
        
        async Task HideDelayAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(_hideMillisecondsDelay, token);
                Hide();
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        void CancelAndDisposeTokenIfNeeded()
        {
            if (_cts == null)
            {
                return;
            }

            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
            _cts.Dispose();
            _cts = null;
        }
    }
}