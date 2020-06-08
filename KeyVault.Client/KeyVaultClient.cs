using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace KeyVault.Client
{
    public class KeyVaultClient : IDisposable
    {
        private static HttpClientHandler HttpHandler;
        private static HttpClient HttpClient;

        private X509Certificate2 ClientCertificate { get; set; }
        private string ApiUrl { get; set; }
        private string ClientId { get; set; }

        public KeyVaultClient(string clientId, X509Certificate2 clientCertificate, string apiUrl = "https://api.keyvault.dev")
        {
            if (!clientCertificate.HasPrivateKey)
            {
                throw new ArgumentException("Missing private key (Required for HttpClient to send the certificate for some reason)");
            }

            (clientCertificate.PrivateKey as RSACng)?.Key.SetProperty(new CngProperty("Export Policy", BitConverter.GetBytes((int)CngExportPolicies.AllowPlaintextExport), CngPropertyOptions.Persist));

            ClientCertificate = clientCertificate;
            ApiUrl = apiUrl;
            ClientId = clientId;

            HttpHandler = new HttpClientHandler()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual
            };

            HttpHandler.ClientCertificates.Add(ClientCertificate);

            HttpHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            HttpClient = new HttpClient(HttpHandler, false) { BaseAddress = new Uri(ApiUrl) };
        }

        internal HttpClient GetApiClient()
        {
            return HttpClient;
        }

        public async Task<string> WhoAmI()
        {
            var hc = GetApiClient();

            var r = await hc.GetAsync($"/api/health/whoami?clientId={ClientId}");
            return await r.Content.ReadAsStringAsync();
        }

        public async Task<bool> SaveSecretAsync(string key, string secretData)
        {
            var hc = GetApiClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "key", $"{ClientCertificate.Thumbprint}::{key}" },
                { "data", Convert.ToBase64String(EncryptSecretData(secretData)) }
            });

            var response = await hc.PostAsync($"/api/data/store?clientId={ClientId}", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<string> GetSecretAsync(string key)
        {
            var hc = GetApiClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "key", $"{ClientCertificate.Thumbprint}::{key}" }
                });

            var response = await hc.PostAsync($"/api/data/fetch?clientId={ClientId}", content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return DecryptSecretData(Convert.FromBase64String(await response.Content.ReadAsStringAsync()));
        }

        internal byte[] EncryptSecretData(string data)
        {
            var cryptoProvider = ClientCertificate.GetRSAPublicKey();
            return cryptoProvider.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
        }

        internal string DecryptSecretData(byte[] data)
        {
            var cryptoProvider = ClientCertificate.GetRSAPrivateKey();
            return Encoding.UTF8.GetString(cryptoProvider.Decrypt(data, RSAEncryptionPadding.Pkcs1));
        }

        public void Dispose()
        {
            HttpHandler?.Dispose();
            HttpClient?.Dispose();
        }
    }
}
