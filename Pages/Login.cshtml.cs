using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;
using RetailEdge.Web.DataAccess;

namespace RetailEdge.Web.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string? Username { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        public string? Message { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                Message = "Username and password are required.";
                return Page();
            }

            try
            {
                using OracleConnection conn = OracleHelper.GetConnection();

                string query = "SELECT PASSWORDHASH, ROLE, NAME FROM SYSTEM.USERS WHERE USERNAME = :username";
                using OracleCommand cmd = new OracleCommand(query, conn);
                cmd.Parameters.Add("username", OracleDbType.Varchar2).Value = Username;

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string dbPassword = reader.GetString(0);
                    string role = reader.GetString(1);
                    string fullName = reader.GetString(2);

                    if (dbPassword == Password)
                    {
                        HttpContext.Session.SetString("user", Username!);
                        HttpContext.Session.SetString("role", role);
                        HttpContext.Session.SetString("name", fullName);
                        return RedirectToPage("/Dashboard");
                    }
                }

                Message = "Invalid username or password.";
                return Page();
            }
            catch (OracleException oex)
            {
                Message = "Login failed (Oracle DB error): " + oex.Message;
                return Page();
            }
            catch (Exception ex)
            {
                Message = "Login failed (General error): " + ex.Message;
                return Page();
            }
        }
    }
}
