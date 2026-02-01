#import <Foundation/Foundation.h>
#import <Security/Security.h>
#import <TargetConditionals.h>
#include <string.h>

#if TARGET_OS_IOS || TARGET_OS_TV

static const char *kDeviceKeyPairTag = "com.ishix.nativebiometricauth.devicekeypair";

static NSData *DeviceKeyPairTagData(const char *tag)
{
    if (tag == NULL)
    {
        tag = kDeviceKeyPairTag;
    }
    return [NSData dataWithBytes:tag length:strlen(tag)];
}
#endif

static SecKeyRef CopyPrivateKey(const char *tag)
{
    NSDictionary *query = @{
        (__bridge id)kSecClass : (__bridge id)kSecClassKey,
        (__bridge id)kSecAttrKeyType : (__bridge id)kSecAttrKeyTypeECSECPrimeRandom,
        (__bridge id)kSecAttrApplicationTag : DeviceKeyPairTagData(tag),
        (__bridge id)kSecReturnRef : @YES
    };

    SecKeyRef key = NULL;
    OSStatus status = SecItemCopyMatching((__bridge CFDictionaryRef)query, (CFTypeRef *)&key);
    if (status != errSecSuccess)
    {
        return NULL;
    }

    return key;
}

static SecKeyRef CreatePrivateKey(const char *tag)
{
    NSDictionary *attributes = @{
        (__bridge id)kSecAttrKeyType : (__bridge id)kSecAttrKeyTypeECSECPrimeRandom,
        (__bridge id)kSecAttrKeySizeInBits : @256,
        (__bridge id)kSecPrivateKeyAttrs : @{
            (__bridge id)kSecAttrIsPermanent : @YES,
            (__bridge id)kSecAttrApplicationTag : DeviceKeyPairTagData(tag),
            (__bridge id)kSecAttrAccessible : (__bridge id)kSecAttrAccessibleAfterFirstUnlockThisDeviceOnly
        }
    };

    CFErrorRef error = NULL;
    SecKeyRef privateKey = SecKeyCreateRandomKey((__bridge CFDictionaryRef)attributes, &error);
    if (error != NULL)
    {
        CFRelease(error);
    }

    return privateKey;
}

static SecKeyRef GetOrCreatePrivateKey(const char *tag)
{
    SecKeyRef key = CopyPrivateKey(tag);
    if (key != NULL)
    {
        return key;
    }

    return CreatePrivateKey(tag);
}

static char *CopyBase64String(NSData *data)
{
    if (data == nil)
    {
        return NULL;
    }

    NSString *base64 = [data base64EncodedStringWithOptions:0];
    return strdup([base64 UTF8String]);
}

static NSData *EncodeLength(NSUInteger length)
{
    if (length < 0x80)
    {
        uint8_t value = (uint8_t)length;
        return [NSData dataWithBytes:&value length:1];
    }
    uint8_t buffer[5];
    NSUInteger count = 0;
    NSUInteger remaining = length;
    while (remaining > 0)
    {
        buffer[count++] = (uint8_t)(remaining & 0xFF);
        remaining >>= 8;
    }
    uint8_t prefix = (uint8_t)(0x80 | count);
    NSMutableData *data = [NSMutableData dataWithBytes:&prefix length:1];
    for (NSInteger i = (NSInteger)count - 1; i >= 0; i--)
    {
        [data appendBytes:&buffer[i] length:1];
    }
    return data;
}

static NSData *BuildEcSpki(NSData *rawPublicKey)
{
    if (rawPublicKey == nil)
    {
        return nil;
    }
    const uint8_t algId[] = {
        0x30, 0x13,
        0x06, 0x07, 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x02, 0x01,
        0x06, 0x08, 0x2A, 0x86, 0x48, 0xCE, 0x3D, 0x03, 0x01, 0x07
    };

    NSMutableData *bitString = [NSMutableData dataWithBytes:"\x03" length:1];
    NSData *bitLength = EncodeLength(rawPublicKey.length + 1);
    [bitString appendData:bitLength];
    uint8_t zero = 0x00;
    [bitString appendBytes:&zero length:1];
    [bitString appendData:rawPublicKey];

    NSUInteger contentLength = sizeof(algId) + bitString.length;
    NSMutableData *result = [NSMutableData dataWithBytes:"\x30" length:1];
    [result appendData:EncodeLength(contentLength)];
    [result appendBytes:algId length:sizeof(algId)];
    [result appendData:bitString];
    return result;
}

extern "C"
{
    bool DKP_HasKeyPairForTag(const char *tag)
    {
        SecKeyRef key = CopyPrivateKey(tag);
        if (key != NULL)
        {
            CFRelease(key);
            return true;
        }

        return false;
    }

    const char *DKP_GetOrCreatePublicKeyBase64ForTag(const char *tag)
    {
        SecKeyRef privateKey = GetOrCreatePrivateKey(tag);
        if (privateKey == NULL)
        {
            return NULL;
        }

        SecKeyRef publicKey = SecKeyCopyPublicKey(privateKey);
        CFRelease(privateKey);
        if (publicKey == NULL)
        {
            return NULL;
        }

        CFErrorRef error = NULL;
        CFDataRef publicData = SecKeyCopyExternalRepresentation(publicKey, &error);
        CFRelease(publicKey);
        if (error != NULL)
        {
            CFRelease(error);
        }

        NSData *rawKey = (__bridge_transfer NSData *)publicData;
        NSData *spki = BuildEcSpki(rawKey);
        return CopyBase64String(spki);
    }

    const char *DKP_GetPublicKeyBase64ForTag(const char *tag)
    {
        SecKeyRef privateKey = CopyPrivateKey(tag);
        if (privateKey == NULL)
        {
            return NULL;
        }

        SecKeyRef publicKey = SecKeyCopyPublicKey(privateKey);
        CFRelease(privateKey);
        if (publicKey == NULL)
        {
            return NULL;
        }

        CFErrorRef error = NULL;
        CFDataRef publicData = SecKeyCopyExternalRepresentation(publicKey, &error);
        CFRelease(publicKey);
        if (error != NULL)
        {
            CFRelease(error);
        }

        NSData *rawKey = (__bridge_transfer NSData *)publicData;
        NSData *spki = BuildEcSpki(rawKey);
        return CopyBase64String(spki);
    }

    const char *DKP_SignBase64ForTag(const char *tag, const unsigned char *data, int length)
    {
        if (data == NULL || length <= 0)
        {
            return NULL;
        }

        SecKeyRef privateKey = CopyPrivateKey(tag);
        if (privateKey == NULL)
        {
            return NULL;
        }

        NSData *payload = [NSData dataWithBytes:data length:(NSUInteger)length];
        CFErrorRef error = NULL;
        CFDataRef signature = SecKeyCreateSignature(
            privateKey,
            kSecKeyAlgorithmECDSASignatureMessageX962SHA256,
            (__bridge CFDataRef)payload,
            &error);
        CFRelease(privateKey);

        if (error != NULL)
        {
            CFRelease(error);
        }

        return CopyBase64String((__bridge_transfer NSData *)signature);
    }

    void DKP_DeleteKeyPairForTag(const char *tag)
    {
        NSDictionary *query = @{
            (__bridge id)kSecClass : (__bridge id)kSecClassKey,
            (__bridge id)kSecAttrKeyType : (__bridge id)kSecAttrKeyTypeECSECPrimeRandom,
            (__bridge id)kSecAttrApplicationTag : DeviceKeyPairTagData(tag)
        };
        SecItemDelete((__bridge CFDictionaryRef)query);
    }

    void DKP_FreeString(const char *str)
    {
        if (str != NULL)
        {
            free((void *)str);
        }
    }
}
