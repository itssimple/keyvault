using Microsoft.Data.Sqlite;
using System;

namespace KeyVault.DatabaseScripts.Models
{
    public class ClientItem
    {
        public string ItemId { get; set; }
        public string ItemData { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public static ClientItem Create(SqliteDataReader reader)
        {
            var item = new ClientItem
            {
                ItemId = reader.GetString(0),
                ItemData = reader.GetString(1),
                CreatedUtc = reader.GetDateTimeOffset(2)
            };

            return item;
        }
    }
}
