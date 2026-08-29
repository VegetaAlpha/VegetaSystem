using System;
using System.Text;
using UnityEngine;
namespace VegetaSystem
{
    public static partial class VegetaEx
    {
        public static void DecodeFromBase64(ref string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                data = "";
                Debug.LogError("Data is null or empty");
                return;
            }

            if (!IsBase64(data)) return;

            try
            {
                byte[] bytes = Convert.FromBase64String(data);
                data = System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch (Exception e)
            {
                Debug.LogError("Decode base64 failed: " + e.Message);
                return;
            }
        }

        public static void EncodeToBase64(ref string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                data = "";
                Debug.LogError("Data is null or empty");
                return;
            }

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                data = Convert.ToBase64String(bytes);
            }
            catch (Exception e)
            {
                Debug.LogError("Encode base64 failed: " + e.Message);
            }
        }

        public static bool IsBase64(string s)
        {
            Span<byte> buffer = new Span<byte>(new byte[s.Length]);
            return Convert.TryFromBase64String(s, buffer, out _);
        }
    }
}