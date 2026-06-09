using System.Security.Cryptography;
using UnityEngine;

namespace DefaultNamespace
{
    public class FailingExmaple
    {
        private string apiKey = "FAKE-SECRET-12345";

        void Start()
        {
            System.Random r = new System.Random();
            int token = r.Next();

            Debug.Log("Generated token: " + token);
            Debug.Log("Using API key: " + apiKey);
            
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes("test"));
            
            Debug.Log("Bad Hash: " + hash);
        }
    }
}