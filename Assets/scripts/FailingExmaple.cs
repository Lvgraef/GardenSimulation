using System.Security.Cryptography;
using UnityEngine;

namespace DefaultNamespace
{
    public class FailingExmaple
    {
        private string apiKey = "ghp_vN7bK9mX2pQ5rT8wW1zY4cC7vB0nMx1qZa2s";

        void Start()
        {
            Debug.Log("Using API key: " + apiKey);
        }
    }
}