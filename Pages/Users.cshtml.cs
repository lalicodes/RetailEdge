using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;
using RetailEdge.Web.DataAccess;

namespace RetailEdge.Web.Pages
{
    public class UsersModel : PageModel
    {
        public List<AppUser> Users { get; set; } = new();
        public List<int> StoreIds { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? StoreFilter { get; set; }

        public void OnGet()
        {
            using var conn = OracleHelper.GetConnection();

            // Load distinct store IDs
            var storeQuery = "SELECT DISTINCT STOREID FROM SYSTEM.USERS WHERE STOREID IS NOT NULL ORDER BY STOREID";
            using (var cmd = new OracleCommand(storeQuery, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    StoreIds.Add(reader.GetInt32(0));
            }

            // Load users
            var userQuery = "SELECT USERID, NAME, ROLE, USERNAME, PERMISSIONS, STOREID FROM SYSTEM.USERS";
            if (StoreFilter.HasValue)
                userQuery += " WHERE STOREID = :store";

            using var userCmd = new OracleCommand(userQuery, conn);
            if (StoreFilter.HasValue)
                userCmd.Parameters.Add("store", StoreFilter.Value);

            using var userReader = userCmd.ExecuteReader();
            while (userReader.Read())
            {
                Users.Add(new AppUser
                {
                    UserId = userReader.GetString(0),
                    Name = userReader.GetString(1),
                    Role = userReader.GetString(2),
                    Username = userReader.GetString(3),
                    Permissions = userReader.GetString(4),
                    StoreId = userReader.IsDBNull(5) ? (int?)null : userReader.GetInt32(5)
                });
            }
        }

        public class AppUser
        {
            public string UserId { get; set; } = "";
            public string Name { get; set; } = "";
            public string Role { get; set; } = "";
            public string Username { get; set; } = "";
            public string Permissions { get; set; } = "";
            public int? StoreId { get; set; }
        }
    }
}
