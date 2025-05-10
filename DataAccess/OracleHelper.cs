using Oracle.ManagedDataAccess.Client;

namespace RetailEdge.Web.DataAccess
{
    public static class OracleHelper
    {
        private static readonly string connStr =
            "User Id=system;" +
            "Password=RetailEdge2025;" +
            "Data Source=192.168.79.103:1521;";

        public static OracleConnection GetConnection()
        {
            var conn = new OracleConnection(connStr);
            conn.Open();
            return conn;
        }
    }
}
