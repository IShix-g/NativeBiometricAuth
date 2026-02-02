
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NativeBiometricAuth
{
    public class BiometricSettings : ScriptableObject
    {
        const string _resourcePath = "NativeBiometricAuth/Settings";
        const string _settingsPath = "Assets/Resources/NativeBiometricAuth/Settings.asset";
        
        public static BiometricSettings Instance
        {
            get
            {
                if (s_instance == null)
                {
                    LoadOrCreate();
                }
                return s_instance;
            }
        }
        static BiometricSettings s_instance;

        [SerializeField, Tooltip("The default language used if the system language is not available in MessageConfigs.")]
        SystemLanguage _defaultLanguage = SystemLanguage.English;
        [SerializeField] List<BiometricMessageSet> _messageConfigs = new();

        /// <summary>
        /// Get the error message based on the failure reason.
        /// </summary>
        public string GetMessage(BiometricFailureReason reason, SystemLanguage lang)
        {
            var config = _messageConfigs.FirstOrDefault(x => x.Language == lang) 
                         ?? _messageConfigs.FirstOrDefault(x => x.Language == _defaultLanguage);
            return config?.GetMessage(reason) ?? "Error: Message not defined.";
        }

        internal static void LoadOrCreate()
        {
            s_instance = Resources.Load<BiometricSettings>(_resourcePath);
#if UNITY_EDITOR
            if (s_instance == null)
            {
                s_instance = CreateInstance();
            }
#endif
        }
        
#if UNITY_EDITOR
        static BiometricSettings CreateInstance()
        {
            var settings = CreateInstance<BiometricSettings>();
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrEmpty(directory)
                && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            AssetDatabase.CreateAsset(settings, _settingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return settings;
        }
        
        [ContextMenu("Setup Default Messages")]
        void Reset()
        {
            _messageConfigs.Clear();
            _messageConfigs.Add(new BiometricMessageSet
            {
                Language = SystemLanguage.Japanese,
                Inactive = "アプリの設定で生体認証がオフになっています。",
                NotSupported = "お使いの端末は生体認証に対応していません。",
                NotConfigured = "端末の設定から生体認証を登録してください。",
                Canceled = "認証がキャンセルされました。",
                AuthenticationFailed = "認証に失敗しました。時間をおいて再度お試しください。",
                SystemError = "一時的なエラーが発生しました。端末を再起動してください。",
                UnexpectedError = "予期せぬエラーが発生しました。"
            });
            _messageConfigs.Add(new BiometricMessageSet
            {
                Language = SystemLanguage.English,
                Inactive = "Biometric authentication is disabled in app settings.",
                NotSupported = "This device does not support biometric authentication.",
                NotConfigured = "Please set up biometrics in your device settings.",
                Canceled = "Authentication was canceled.",
                AuthenticationFailed = "Authentication failed. Please try again later.",
                SystemError = "A system error occurred. Please restart your device.",
                UnexpectedError = "An unexpected error occurred."
            });
            _messageConfigs.Add(new BiometricMessageSet
            {
                Language = SystemLanguage.ChineseSimplified,
                Inactive = "应用设置中已禁用生物识别身份验证。",
                NotSupported = "此设备不支持生物识别身份验证。",
                NotConfigured = "请在设备设置中设置生物识别。",
                Canceled = "身份验证已取消。",
                AuthenticationFailed = "身份验证失败。请稍后再试。",
                SystemError = "发生系统错误。请重新启动您的设备。",
                UnexpectedError = "发生意外错误。"
            });
            _messageConfigs.Add(new BiometricMessageSet
            {
                Language = SystemLanguage.ChineseTraditional,
                Inactive = "應用程式設定中已禁用生物辨識身分驗證。",
                NotSupported = "此裝置不支援生物辨識身分驗證。",
                NotConfigured = "請在裝置設定中設定生物辨識。",
                Canceled = "身分驗證已取消。",
                AuthenticationFailed = "身分驗證失敗。請稍後再試。",
                SystemError = "發生系統錯誤。請重新啟動您的裝置。",
                UnexpectedError = "發生意外錯誤。"
            });
            _messageConfigs.Add(new BiometricMessageSet
            {
                Language = SystemLanguage.Korean,
                Inactive = "앱 설정에서 생체 인증이 비활성화되어 있습니다.",
                NotSupported = "이 기기는 생체 인증을 지원하지 않습니다.",
                NotConfigured = "기기 설정에서 생체 인증을 등록해 주세요.",
                Canceled = "인증이 취소되었습니다.",
                AuthenticationFailed = "인증에 실패했습니다. 잠시 후 다시 시도해 주세요.",
                SystemError = "시스템 오류가 발생했습니다. 기기를 재부팅해 주세요.",
                UnexpectedError = "예기치 않은 오류가 발생했습니다."
            });
            _messageConfigs.Add(new BiometricMessageSet
            {
                Language = SystemLanguage.French,
                Inactive = "L'authentification biométrique est désactivée dans les paramètres de l'application.",
                NotSupported = "Cet appareil ne prend pas en charge l'authentification biométrique.",
                NotConfigured = "Veuillez configurer la biométrie dans les paramètres de votre appareil.",
                Canceled = "L'authentification a été annulée.",
                AuthenticationFailed = "L'authentification a échoué. Veuillez réessayer plus tard.",
                SystemError = "Une erreur système s'est produite. Veuillez redémarrer votre appareil.",
                UnexpectedError = "Une erreur inattendue s'est produite."
            });
            _messageConfigs.Add(new BiometricMessageSet
            {
                Language = SystemLanguage.German,
                Inactive = "Die biometrische Authentifizierung ist in den App-Einstellungen deaktiviert.",
                NotSupported = "Dieses Gerät unterstützt keine biometrische Authentifizierung.",
                NotConfigured = "Bitte richten Sie die Biometrie in Ihren Geräteeinstellungen ein.",
                Canceled = "Authentifizierung wurde abgebrochen.",
                AuthenticationFailed = "Authentifizierung fehlgeschlagen. Bitte versuchen Sie es später noch einmal.",
                SystemError = "Ein Systemfehler ist aufgetreten. Bitte starten Sie Ihr Gerät neu.",
                UnexpectedError = "Ein unerwarteter Fehler ist aufgetreten."
            });
            _messageConfigs.Add(new BiometricMessageSet
            {
                Language = SystemLanguage.Spanish,
                Inactive = "La autenticación biométrica está desactivada en los ajustes de la aplicación.",
                NotSupported = "Este dispositivo no admite la autenticación biométrica.",
                NotConfigured = "Configure la biometría en los ajustes de su dispositivo.",
                Canceled = "Autenticación cancelada.",
                AuthenticationFailed = "Error de autenticación. Inténtelo de nuevo más tarde.",
                SystemError = "Se produjo un error del sistema. Reinicie su dispositivo.",
                UnexpectedError = "Se ha producido un error inesperado."
            });
            _messageConfigs.Add(new BiometricMessageSet
            {
                Language = SystemLanguage.Thai,
                Inactive = "การยืนยันตัวตนด้วยชีวมาตรถูกปิดใช้งานในการตั้งค่าแอป",
                NotSupported = "อุปกรณ์นี้ไม่รองรับการยืนยันตัวตนด้วยชีวมาตร",
                NotConfigured = "โปรดตั้งค่าชีวมาตรในการตั้งค่าอุปกรณ์ของคุณ",
                Canceled = "การยืนยันตัวตนถูกยกเลิก",
                AuthenticationFailed = "การยืนยันตัวตนล้มเหลว โปรดลองอีกครั้งในภายหลัง",
                SystemError = "เกิดข้อผิดพลาดของระบบ โปรดรีสตาร์ทอุปกรณ์ของคุณ",
                UnexpectedError = "เกิดข้อผิดพลาดที่ไม่คาดคิด"
            });
        }
#endif
    }
}