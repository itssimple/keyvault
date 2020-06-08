using KeyVault.DatabaseScripts;
using KeyVault.DatabaseScripts.Models;
using KeyVault.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace KeyVault.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly Database db;

        public DataController(Database _db)
        {
            db = _db;
        }

        [HttpPost("store")]
        public bool StoreSecret()
        {
            if (!HelpUtils.IsValidClient(Request.Form["key"].ToString(), User))
                return false;

            db.ExecuteNonQuery("DELETE FROM ClientItems WHERE itemId = $itemId", new Microsoft.Data.Sqlite.SqliteParameter("itemId", Request.Form["key"].ToString()));

            return db.ExecuteNonQuery(
                "INSERT INTO ClientItems (itemId, itemData, created_utc) VALUES ($itemId, $itemData, $created)",
                new Microsoft.Data.Sqlite.SqliteParameter("itemId", Request.Form["key"].ToString()),
                new Microsoft.Data.Sqlite.SqliteParameter("itemData", Request.Form["data"].ToString()),
                new Microsoft.Data.Sqlite.SqliteParameter("created", DateTimeOffset.UtcNow)
            ) == 1;
        }

        [HttpPost("fetch")]
        public string FetchSecret()
        {
            if (!HelpUtils.IsValidClient(Request.Form["key"].ToString(), User))
                return null;

            var item = db.GetSingleItem(ClientItem.Create, "SELECT itemId, itemData, created_utc FROM ClientItems WHERE itemId = $itemId", new Microsoft.Data.Sqlite.SqliteParameter("itemId", Request.Form["key"].ToString()));

            if (item != null)
            {
                return item.ItemData;
            }

            return null;
        }
    }
}
