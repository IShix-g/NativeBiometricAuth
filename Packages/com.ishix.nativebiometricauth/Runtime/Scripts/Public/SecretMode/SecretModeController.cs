
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NativeBiometricAuth
{
    internal sealed class SecretModeController : MonoBehaviour
    {
        const int _authDelayMilliseconds = 500;
        
        bool _isAuthenticating;
        bool _hasPaused;
        DateTime _lastPausedTime;
        DateTime _lastResumedTime;
        bool _ignoreNextResume;
        CancellationTokenSource _authCts;

        public void Start()
        {
            Biometric.OnActiveChanged += HandleActiveChanged;
            SecretModePrivacyScreen.SetEnabled(SecretMode.IsActive);
        }

        void OnDestroy()
        {
            Biometric.OnActiveChanged -= HandleActiveChanged;
            SecretModePrivacyScreen.SetEnabled(false);
            CancelAuthTask();
        }

        void OnApplicationPause(bool pauseStatus)
        {
#if !UNITY_STANDALONE_OSX
            if (pauseStatus)
            {
                _hasPaused = true;
                _lastPausedTime = DateTime.UtcNow;
                ShowOverlayIfNeeded();
            }
            else
            {
                _lastResumedTime = DateTime.UtcNow;
                HandleResume();
            }
#endif
        }

        void OnApplicationFocus(bool hasFocus)
        {
#if UNITY_STANDALONE_OSX
            if (!hasFocus)
            {
                if (_isAuthenticating)
                {
                    return;
                }

                if ((DateTime.UtcNow - _lastResumedTime).TotalMilliseconds < 1000)
                {
                    return;
                }

                _hasPaused = true;
                _lastPausedTime = DateTime.UtcNow;
                ShowOverlayIfNeeded();
            }
            else
            {
                _lastResumedTime = DateTime.UtcNow;
                HandleResume();
            }
#endif
        }

        void HandleActiveChanged(bool isActive)
        {
            SecretMode.NotifySecretModeActiveChanged(isActive);
            SecretModePrivacyScreen.SetEnabled(isActive);
            if (!isActive)
            {
                HideOverlay();
                CancelAuthTask();
            }
        }

        public void ShowAndAuthenticate()
        {
            if (_isAuthenticating)
            {
                return;
            }
            if (!SecretMode.ShouldUseBiometric())
            {
                HideOverlay();
                return;
            }

            ShowOverlay();
            StartAuthenticateNextFrame();
        }

        void Authenticate()
        {
            CancelAuthTask();
            if (_isAuthenticating)
            {
                return;
            }
            _isAuthenticating = true;
            _ignoreNextResume = true;
            Biometric.Authenticate(
                () =>
                {
                    _lastResumedTime = DateTime.UtcNow;
                    _isAuthenticating = false;
                    HideOverlay();
                    SecretMode.NotifyAuthenticateSuccess();
                },
                reason =>
                {
                    _lastResumedTime = DateTime.UtcNow;
                    _isAuthenticating = false;
                    SecretMode.NotifyAuthenticateFailure(reason);
                },
                SecretMode.AllowDeviceCredential);
        }

        void ShowOverlayIfNeeded()
        {
            if (!SecretMode.ShouldUseBiometric())
            {
                HideOverlay();
                return;
            }
            ShowOverlay();
        }

        void ShowOverlay() => SecretMode.ObjectController.Show();

        void HideOverlay() => SecretMode.ObjectController.Hide();

        void StartAuthenticateNextFrame()
        {
            CancelAuthTask();
            _ = AuthenticateNextFrameAsync();
        }
        
        async Task AuthenticateNextFrameAsync(CancellationToken token = default)
        {
            try
            {
                _authCts = CancellationTokenSource.CreateLinkedTokenSource(token, destroyCancellationToken);
                await Task.Delay(_authDelayMilliseconds, _authCts.Token);
                Authenticate();
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        
        void CancelAuthTask()
        {
            if (_authCts == null)
            {
                return;
            }
            try
            {
                if (!_authCts.IsCancellationRequested)
                {
                    _authCts.Cancel();
                }
                _authCts.Dispose();
                _authCts = null;
            }
            catch
            {
                // Ignored
            }
        }
        
        void HandleResume()
        {
            if (SecretMode.ConsumeSkipOverlayOnActivate())
            {
                _hasPaused = false;
                return;
            }
            if (_isAuthenticating)
            {
                return;
            }
            if (_ignoreNextResume)
            {
                _ignoreNextResume = false;
                return;
            }
            if (!SecretMode.ShouldUseBiometric())
            {
                HideOverlay();
                return;
            }

            if (_hasPaused)
            {
                _hasPaused = false;
                var elapsed = (DateTime.UtcNow - _lastPausedTime).TotalSeconds;
                if (elapsed < SecretMode.ResumeGraceSeconds)
                {
                    HideOverlay();
                    return;
                }
            }
            ShowAndAuthenticate();
        }
    }
}