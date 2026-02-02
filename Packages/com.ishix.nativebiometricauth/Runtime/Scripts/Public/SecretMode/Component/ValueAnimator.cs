
using System;
using UnityEngine;

namespace NativeBiometricAuth
{
    internal class ValueAnimator : MonoBehaviour
    {
        public enum EaseType
        {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseOutBack
        }

        public bool IsPlaying { get; private set; }
        float _timer;

        float _startValue;
        float _targetValue;
        float _duration;
        EaseType _easeType;

        Action<float> _onUpdate;
        Action _onComplete;
        
        public void Play(
            float startValue,
            float targetValue,
            float duration,
            Action<float> onUpdate,
            Action onComplete = null,
            EaseType easeType = EaseType.EaseOutQuad) 
        {
            _startValue = startValue;
            _targetValue = targetValue;
            _duration = duration;
            _onUpdate = onUpdate;
            _onComplete = onComplete;
            _easeType = easeType;
            
            _timer = 0f;
            IsPlaying = true;
            
            if (_duration <= 0f) 
            {
                CompleteImmediately();
            }
        }
        
        public void Stop()
        {
            IsPlaying = false;
            _onUpdate = null;
            _onComplete = null;
        }

        void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            _timer += Time.deltaTime;
            var t = _timer / _duration;

            if (t >= 1.0f)
            {
                CompleteImmediately();
            }
            else
            {
                var easedT = EvaluateEase(t, _easeType);
                var currentValue = Mathf.Lerp(_startValue, _targetValue, easedT);
                _onUpdate?.Invoke(currentValue);
            }
        }
        
        void CompleteImmediately()
        {
            IsPlaying = false;
            _onUpdate?.Invoke(_targetValue);
            _onComplete?.Invoke();
            _onUpdate = null;
            _onComplete = null;
        }
        
        float EvaluateEase(float t, EaseType type)
        {
            switch (type)
            {
                case EaseType.Linear:
                    return t;
                case EaseType.EaseInQuad:
                    return t * t;
                case EaseType.EaseOutQuad:
                    return t * (2f - t);
                case EaseType.EaseInOutQuad:
                    return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
                case EaseType.EaseOutBack:
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
                default:
                    return t;
            }
        }
    }
}