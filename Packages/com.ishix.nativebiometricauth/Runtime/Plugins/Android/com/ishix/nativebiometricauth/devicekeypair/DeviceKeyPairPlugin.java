package com.ishix.nativebiometricauth.devicekeypair;

import android.os.Build;
import android.util.Base64;

import java.math.BigInteger;
import java.security.KeyPair;
import java.security.KeyPairGenerator;
import java.security.KeyStore;
import java.security.PrivateKey;
import java.security.Signature;
import java.security.spec.ECGenParameterSpec;
import java.util.Calendar;

import javax.security.auth.x500.X500Principal;

import com.unity3d.player.UnityPlayer;

import android.security.KeyPairGeneratorSpec;
import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;

public final class DeviceKeyPairPlugin {
    private static final String KEYSTORE = "AndroidKeyStore";
    private static final String ALIAS = "com.ishix.nativebiometricauth.devicekeypair";

    private DeviceKeyPairPlugin() {
    }

    public static boolean hasKeyPair() {
        return hasKeyPair(null);
    }

    public static boolean hasKeyPair(String alias) {
        try {
            KeyStore keyStore = KeyStore.getInstance(KEYSTORE);
            keyStore.load(null);
            return keyStore.containsAlias(normalizeAlias(alias));
        } catch (Exception ignored) {
            return false;
        }
    }

    public static String getOrCreatePublicKeyBase64() {
        return getOrCreatePublicKeyBase64(null);
    }

    public static String getOrCreatePublicKeyBase64(String alias) {
        try {
            KeyPair keyPair = getOrCreateKeyPair(normalizeAlias(alias));
            if (keyPair == null) {
                return null;
            }
            return encodePublicKey(keyPair.getPublic());
        } catch (Exception ignored) {
            return null;
        }
    }

    public static String getPublicKeyBase64() {
        return getPublicKeyBase64(null);
    }

    public static String getPublicKeyBase64(String alias) {
        try {
            KeyPair keyPair = getKeyPair(normalizeAlias(alias));
            if (keyPair == null) {
                return null;
            }
            return encodePublicKey(keyPair.getPublic());
        } catch (Exception ignored) {
            return null;
        }
    }

    public static String signBase64(byte[] data) {
        return signBase64(null, data);
    }

    public static String signBase64(String alias, byte[] data) {
        if (data == null || data.length == 0) {
            return null;
        }

        try {
            KeyPair keyPair = getKeyPair(normalizeAlias(alias));
            if (keyPair == null) {
                return null;
            }

            PrivateKey privateKey = keyPair.getPrivate();
            String algorithm = privateKey.getAlgorithm();
            String signatureAlgorithm = "EC".equalsIgnoreCase(algorithm)
                ? "SHA256withECDSA"
                : "SHA256withRSA";
            Signature signature = Signature.getInstance(signatureAlgorithm);
            signature.initSign(privateKey);
            signature.update(data);
            byte[] signed = signature.sign();
            return Base64.encodeToString(signed, Base64.NO_WRAP);
        } catch (Exception ignored) {
            return null;
        }
    }

    public static void deleteKeyPair() {
        deleteKeyPair(null);
    }

    public static void deleteKeyPair(String alias) {
        try {
            KeyStore keyStore = KeyStore.getInstance(KEYSTORE);
            keyStore.load(null);
            keyStore.deleteEntry(normalizeAlias(alias));
        } catch (Exception ignored) {
        }
    }

    private static KeyPair getOrCreateKeyPair(String alias) throws Exception {
        KeyPair existing = getKeyPair(alias);
        if (existing != null) {
            return existing;
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            KeyPairGenerator generator = KeyPairGenerator.getInstance(
                KeyProperties.KEY_ALGORITHM_EC,
                KEYSTORE
            );

            KeyGenParameterSpec spec = new KeyGenParameterSpec.Builder(
                alias,
                KeyProperties.PURPOSE_SIGN | KeyProperties.PURPOSE_VERIFY
            )
                .setAlgorithmParameterSpec(new ECGenParameterSpec("secp256r1"))
                .setDigests(KeyProperties.DIGEST_SHA256)
                .setUserAuthenticationRequired(false)
                .build();

            generator.initialize(spec);
            return generator.generateKeyPair();
        }

        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.JELLY_BEAN_MR2) {
            return null;
        }

        Calendar start = Calendar.getInstance();
        Calendar end = Calendar.getInstance();
        end.add(Calendar.YEAR, 20);

        KeyPairGeneratorSpec spec = new KeyPairGeneratorSpec.Builder(UnityPlayer.currentActivity.getApplicationContext())
            .setAlias(alias)
            .setSubject(new X500Principal("CN=" + alias))
            .setSerialNumber(BigInteger.ONE)
            .setStartDate(start.getTime())
            .setEndDate(end.getTime())
            .build();

        KeyPairGenerator generator = KeyPairGenerator.getInstance("RSA", KEYSTORE);
        generator.initialize(spec);
        return generator.generateKeyPair();
    }

    private static KeyPair getKeyPair(String alias) throws Exception {
        KeyStore keyStore = KeyStore.getInstance(KEYSTORE);
        keyStore.load(null);
        KeyStore.Entry entry = keyStore.getEntry(alias, null);
        if (!(entry instanceof KeyStore.PrivateKeyEntry)) {
            return null;
        }

        KeyStore.PrivateKeyEntry keyEntry = (KeyStore.PrivateKeyEntry) entry;
        return new KeyPair(keyEntry.getCertificate().getPublicKey(), keyEntry.getPrivateKey());
    }

    private static String encodePublicKey(java.security.PublicKey publicKey) {
        if (publicKey == null) {
            return null;
        }
        byte[] encoded = publicKey.getEncoded();
        return encoded == null ? null : Base64.encodeToString(encoded, Base64.NO_WRAP);
    }

    public static String getKeyAlgorithm() {
        return getKeyAlgorithm(null);
    }

    public static String getKeyAlgorithm(String alias) {
        try {
            KeyPair keyPair = getOrCreateKeyPair(normalizeAlias(alias));
            if (keyPair == null || keyPair.getPrivate() == null) {
                return null;
            }
            return keyPair.getPrivate().getAlgorithm();
        } catch (Exception ignored) {
            return null;
        }
    }

    private static String normalizeAlias(String alias) {
        return (alias == null || alias.isEmpty()) ? ALIAS : alias;
    }
}
