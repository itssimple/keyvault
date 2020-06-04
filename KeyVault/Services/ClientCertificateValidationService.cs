using KeyVault.DatabaseScripts;
using KeyVault.DatabaseScripts.Models;
using System.Security.Cryptography.X509Certificates;

namespace KeyVault.Services
{
    public class ClientCertificateValidationService
    {
        private readonly Database _db;

        public ClientCertificateValidationService(Database db)
        {
            _db = db;
        }

        public bool ValidateCertificate(string clientId, X509Certificate2 clientCertificate)
        {
            var potentialClient = _db.GetSingleItem(
                Client.Create,
                "SELECT clientId, certificateThumbprint, created_utc, disabled, disabledDate FROM Clients WHERE clientId = $clientId AND disabled = 0",
                new Microsoft.Data.Sqlite.SqliteParameter("clientId", clientId)
            );

            if (potentialClient is Client && clientCertificate.Thumbprint == potentialClient.CertificateThumbprint)
            {
                return true;
            }

            return false;
        }
    }
}
