using UnityEngine;

public static class SaveSystem
{
    private static readonly string SecretKey = "DotChainRushKey!"; // XOR encryption key for local data integrity

    public static void SaveInt(string key, int value)
    {
        string raw = value.ToString();
        string encrypted = EncryptDecrypt(raw);
        PlayerPrefs.SetString(key, encrypted);
        PlayerPrefs.Save();
    }

    public static int LoadInt(string key, int defaultValue)
    {
        if (!PlayerPrefs.HasKey(key)) return defaultValue;
        try
        {
            string encrypted = PlayerPrefs.GetString(key);
            string decrypted = EncryptDecrypt(encrypted);
            return int.Parse(decrypted);
        }
        catch
        {
            Debug.LogWarning($"[SaveSystem] Failed to decrypt or parse value for key: {key}. Resetting to default.");
            return defaultValue;
        }
    }

    public static void SaveFloat(string key, float value)
    {
        string raw = value.ToString("F3");
        string encrypted = EncryptDecrypt(raw);
        PlayerPrefs.SetString(key, encrypted);
        PlayerPrefs.Save();
    }

    public static float LoadFloat(string key, float defaultValue)
    {
        if (!PlayerPrefs.HasKey(key)) return defaultValue;
        try
        {
            string encrypted = PlayerPrefs.GetString(key);
            string decrypted = EncryptDecrypt(encrypted);
            return float.Parse(decrypted);
        }
        catch
        {
            Debug.LogWarning($"[SaveSystem] Failed to decrypt or parse value for key: {key}. Resetting to default.");
            return defaultValue;
        }
    }

    private static string EncryptDecrypt(string text)
    {
        char[] result = new char[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            result[i] = (char)(text[i] ^ SecretKey[i % SecretKey.Length]);
        }
        return new string(result);
    }
}
