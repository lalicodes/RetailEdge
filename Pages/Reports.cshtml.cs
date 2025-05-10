using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;
using RetailEdge.Web.DataAccess;
using System.Text.Json;

namespace RetailEdge.Web.Pages
{
    public class ReportsModel : PageModel
    {
        public string[] Dates { get; set; } = Array.Empty<string>();
        public decimal[] DailySales { get; set; } = Array.Empty<decimal>();

        public string[] Categories { get; set; } = Array.Empty<string>();
        public decimal[] CategorySales { get; set; } = Array.Empty<decimal>();

        public List<TopProduct> TopProducts { get; set; } = new();

        public class TopProduct
        {
            public string ProductName { get; set; } = "";
            public int Quantity { get; set; }
            public decimal Revenue { get; set; }
        }

        public void OnGet()
        {
            using var conn = OracleHelper.GetConnection();

            // 1. Daily Sales
            var dateList = new List<string>();
            var salesList = new List<decimal>();
            var dailyQuery = @"
                SELECT TO_CHAR(ORDER_DATE, 'YYYY-MM-DD') AS ODATE, SUM(TOTAL_AMOUNT)
                FROM SYSTEM.ORDERS
                WHERE ORDER_DATE >= SYSDATE - 14
                GROUP BY TO_CHAR(ORDER_DATE, 'YYYY-MM-DD')
                ORDER BY ODATE";

            using (var cmd = new OracleCommand(dailyQuery, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    dateList.Add(reader.GetString(0));
                    salesList.Add(reader.GetDecimal(1));
                }
            }
            Dates = dateList.ToArray();
            DailySales = salesList.ToArray();

            // 2. Category Sales
            var catList = new List<string>();
            var catSales = new List<decimal>();
            var catQuery = @"
                SELECT C.CATEGORYNAME, SUM(I.UNIT_PRICE * I.QUANTITY)
                FROM SYSTEM.ORDER_ITEMS I
                JOIN SYSTEM.PRODUCTS P ON I.PRODUCT_ID = P.PRODUCTID
                JOIN SYSTEM.CATEGORIES C ON P.CATEGORY = C.CATEGORYID
                GROUP BY C.CATEGORYNAME";

            using (var cmd = new OracleCommand(catQuery, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    catList.Add(reader.GetString(0));
                    catSales.Add(reader.GetDecimal(1));
                }
            }
            Categories = catList.ToArray();
            CategorySales = catSales.ToArray();

            // 3. Top Products
            string topQuery = @"
                SELECT P.PRODUCTNAME, SUM(I.QUANTITY), SUM(I.UNIT_PRICE * I.QUANTITY)
                FROM SYSTEM.ORDER_ITEMS I
                JOIN SYSTEM.PRODUCTS P ON I.PRODUCT_ID = P.PRODUCTID
                GROUP BY P.PRODUCTNAME
                ORDER BY SUM(I.QUANTITY) DESC
                FETCH FIRST 5 ROWS ONLY";

            using (var cmd = new OracleCommand(topQuery, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    TopProducts.Add(new TopProduct
                    {
                        ProductName = reader.GetString(0),
                        Quantity = reader.GetInt32(1),
                        Revenue = reader.GetDecimal(2)
                    });
                }
            }
        }
    }
}
