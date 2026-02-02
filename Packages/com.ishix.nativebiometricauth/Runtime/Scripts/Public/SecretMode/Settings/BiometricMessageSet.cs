
using System;
using UnityEngine;

namespace NativeBiometricAuth
{
    [Serializable]
    public class BiometricMessageSet
    {
        public SystemLanguage Language;
        public string Inactive;
        public string NotSupported;
        public string NotConfigured;
        public string Canceled;
        public string AuthenticationFailed;
        public string SystemError;
        public string UnexpectedError;

        [Tooltip("Get the message corresponding to the failure reason.")]
        public string GetMessage(BiometricFailureReason reason)
        {
            return reason switch
            {
                BiometricFailureReason.Inactive => Inactive,
                BiometricFailureReason.NotSupported => NotSupported,
                BiometricFailureReason.NotConfigured => NotConfigured,
                BiometricFailureReason.Canceled => Canceled,
                BiometricFailureReason.AuthenticationFailed => AuthenticationFailed,
                BiometricFailureReason.SystemError => SystemError,
                _ => UnexpectedError
            };
        }
    }
}