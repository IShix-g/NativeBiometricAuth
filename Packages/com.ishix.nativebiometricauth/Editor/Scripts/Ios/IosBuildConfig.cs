
using System;
using UnityEngine;

namespace NativeBiometricAuth.Editor
{
    [Serializable]
    internal sealed class IosBuildConfig
    {
        public const string DefaultNsFaceIDUsageDescription = "This app uses Face ID to unlock features securely.";

        [SerializeField]
        [TextArea(2, 5)]
        [Tooltip("This string will be added to Info.plist(iOS) under NSFaceIDUsageDescription key.")]
        string _faceIDUsageDescription = DefaultNsFaceIDUsageDescription;

        public string NsFaceIDUsageDescription => _faceIDUsageDescription;
        
        internal IosBuildConfig() { }

        internal IosBuildConfig(string description) => _faceIDUsageDescription = description;
        
        public static IosBuildConfig CreateDefault() => new ();
    }
}
