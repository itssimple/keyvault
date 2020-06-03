using Microsoft.Data.Sqlite;
using System;

namespace KeyVault.DatabaseScripts.Models
{
    public class Client
    {
        public Guid ClientId { get; set; }
        public string CertificateThumbprint { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public bool Disabled { get; set; }
        public DateTimeOffset DisabledDate { get; set; }

        public static Client Create(SqliteDataReader reader)
        {
            var item = new Client
            {
                ClientId = reader.GetGuid(0),
                CertificateThumbprint = reader.GetString(1),
                CreatedUtc = reader.GetDateTimeOffset(2),
                Disabled = reader.GetBoolean(3),
                DisabledDate = reader.GetDateTimeOffset(4)
            };

            return item;
        }
    }
}
