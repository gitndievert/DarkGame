using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace Dark.Utility.Encryption
{
    public class EncryptedSerialization : MonoBehaviour
    {
        private const string encryptionKey = "YourEncryptionKey";

        private void Start()
        {
            Texture2D texture = LoadTextureFromFile("path_to_your_texture.png");

            // Convert Texture2D to byte array
            byte[] textureData = texture.EncodeToPNG();

            // Encrypt byte array
            byte[] encryptedData = EncryptData(textureData, encryptionKey);

            // Serialize encrypted byte array
            SerializeData(encryptedData, "texture_data.dat");

            // Deserialize byte array
            byte[] deserializedData = DeserializeData("texture_data.dat");

            // Decrypt byte array
            byte[] decryptedData = DecryptData(deserializedData, encryptionKey);

            // Convert byte array back to Texture2D
            Texture2D deserializedTexture = new Texture2D(2, 2);
            deserializedTexture.LoadImage(decryptedData);

            // Use the deserializedTexture as needed
        }

        Texture2D LoadTextureFromFile(string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }

        void SerializeData(byte[] data, string filePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(filePath, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
        }

        byte[] DeserializeData(string filePath)
        {
            if (File.Exists(filePath))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(filePath, FileMode.Open);
                byte[] data = (byte[])formatter.Deserialize(stream);
                stream.Close();
                return data;
            }
            else
            {
                Debug.LogError("File not found: " + filePath);
                return null;
            }
        }

        byte[] EncryptData(byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
                aes.IV = aes.Key;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    return ms.ToArray();
                }
            }
        }

        byte[] DecryptData(byte[] data, string key)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
                aes.IV = aes.Key;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}