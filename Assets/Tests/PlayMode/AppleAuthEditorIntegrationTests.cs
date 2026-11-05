#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class AppleAuthEditorIntegrationTests
{
    [UnityTest]
    public IEnumerator DevMockResponse_SetsUnityLoginStateAndSessionToken()
    {
        string[] stringKeys = { "AccessToken", "RefreshToken", "UserID" };
        bool[] hadStringKeys = new bool[stringKeys.Length];
        string[] previousStrings = new string[stringKeys.Length];
        for (int i = 0; i < stringKeys.Length; i++)
        {
            hadStringKeys[i] = PlayerPrefs.HasKey(stringKeys[i]);
            previousStrings[i] = PlayerPrefs.GetString(stringKeys[i], "");
            PlayerPrefs.DeleteKey(stringKeys[i]);
        }
        bool hadMockFlag = PlayerPrefs.HasKey("IsMockAppleAuth");
        int previousMockFlag = PlayerPrefs.GetInt("IsMockAppleAuth", 0);
        PlayerPrefs.DeleteKey("IsMockAppleAuth");

        GameObject gameObject = null;
        try
        {
            Assembly gameAssembly = Assembly.Load("Assembly-CSharp");
            Type managerType = gameAssembly.GetType("AppleAuthManager");
            Assert.IsNotNull(managerType, "AppleAuthManager was not found in Assembly-CSharp.");

            gameObject = new GameObject("AppleAuthEditorIntegrationTest");
            Component manager = gameObject.AddComponent(managerType);
            const string rawNonce = "unity-nunit-apple-nonce";
            string identityToken = CreateMockIdentityToken(rawNonce);
            MethodInfo submitMethod = managerType.GetMethod("SubmitIdentityTokenForEditorTest");
            Assert.IsNotNull(submitMethod, "Editor test submit hook was not found.");

            IEnumerator submit = (IEnumerator)submitMethod.Invoke(
                manager,
                new object[] { identityToken, rawNonce });
            yield return ((MonoBehaviour)manager).StartCoroutine(submit);

            PropertyInfo loggedIn = managerType.GetProperty("IsLoggedIn");
            Assert.IsTrue((bool)loggedIn.GetValue(manager), "Auth manager did not enter LoggedIn state.");
            Assert.AreEqual(identityToken, PlayerPrefs.GetString("AccessToken"));
            Assert.IsFalse(string.IsNullOrEmpty(PlayerPrefs.GetString("UserID")));
            Assert.AreEqual(1, PlayerPrefs.GetInt("IsMockAppleAuth"));
        }
        finally
        {
            if (gameObject != null)
                UnityEngine.Object.DestroyImmediate(gameObject);
            for (int i = 0; i < stringKeys.Length; i++)
            {
                if (hadStringKeys[i])
                    PlayerPrefs.SetString(stringKeys[i], previousStrings[i]);
                else
                    PlayerPrefs.DeleteKey(stringKeys[i]);
            }
            if (hadMockFlag)
                PlayerPrefs.SetInt("IsMockAppleAuth", previousMockFlag);
            else
                PlayerPrefs.DeleteKey("IsMockAppleAuth");
            PlayerPrefs.Save();
        }
    }

    private static string CreateMockIdentityToken(string rawNonce)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string hashedNonce;
        using (SHA256 sha = SHA256.Create())
            hashedNonce = ToHex(sha.ComputeHash(Encoding.UTF8.GetBytes(rawNonce)));

        const string header = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
        string payload = "{\"iss\":\"https://appleid.apple.com.mock\"," +
                         "\"aud\":\"com.platusalmon.game\",\"sub\":\"unity-nunit-user\"," +
                         "\"email\":\"unity-test@example.invalid\",\"nonce\":\"" + hashedNonce + "\"," +
                         "\"iat\":" + now + ",\"exp\":" + (now + 300) + "}";
        return Base64Url(Encoding.UTF8.GetBytes(header)) + "." +
               Base64Url(Encoding.UTF8.GetBytes(payload)) + ".";
    }

    private static string ToHex(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (byte value in bytes)
            builder.Append(value.ToString("x2"));
        return builder.ToString();
    }

    private static string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
#endif
