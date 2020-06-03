using KeyVault.DatabaseScripts;
using System;

namespace KeyVault.DataLayer
{
    public class StorageLayer : IDisposable
    {
        internal Database DB { get; private set; }
        public StorageLayer(Database db)
        {
            DB = db;
        }

        public void Dispose()
        {
            DB?.Dispose();
        }
    }
}
