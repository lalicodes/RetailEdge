using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;
using RetailEdge.Web.DataAccess;

namespace RetailEdge.Web.Pages
{
    public class ProductsModel : PageModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public void OnGet()
        {
            using var conn = OracleHelper.GetConnection();

            string productQuery = @"
                SELECT P.PRODUCTID, P.PRODUCTNAME, P.SKU, P.UNITPRICE, P.INSTOCKQTY, C.CATEGORYNAME
                FROM SYSTEM.PRODUCTS P
                LEFT JOIN SYSTEM.CATEGORIES C ON P.CATEGORY = C.CATEGORYID
                WHERE P.ISACTIVE = 'Yes'";

            using (var cmd = new OracleCommand(productQuery, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Products.Add(new Product
                    {
                        ProductId = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        SKU = reader.GetString(2),
                        UnitPrice = reader.GetDecimal(3),
                        InStockQty = reader.GetInt32(4),
                        Category = reader.GetString(5)
                    });
                }
            }

            string categoryQuery = "SELECT CATEGORYID, CATEGORYNAME FROM SYSTEM.CATEGORIES WHERE ISACTIVE = 'Yes'";
            using (var cmd = new OracleCommand(categoryQuery, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Categories.Add(new Category
                    {
                        CategoryId = reader.GetString(0),
                        CategoryName = reader.GetString(1)
                    });
                }
            }
        }

        public IActionResult OnPostAdd(string Name, string SKU, decimal UnitPrice, int InStockQty, string Category)
        {
            try
            {
                using var conn = OracleHelper.GetConnection();
                string query = @"INSERT INTO SYSTEM.PRODUCTS 
                                (PRODUCTID, PRODUCTNAME, SKU, UNITPRICE, INSTOCKQTY, CATEGORY, ISACTIVE)
                                VALUES (PRODUCTS_SEQ.NEXTVAL, :name, :sku, :price, :qty, :category, 'Yes')";

                using var cmd = new OracleCommand(query, conn);
                cmd.Parameters.Add("name", Name);
                cmd.Parameters.Add("sku", SKU);
                cmd.Parameters.Add("price", UnitPrice);
                cmd.Parameters.Add("qty", InStockQty);
                cmd.Parameters.Add("category", Category);
                cmd.ExecuteNonQuery();

                TempData["Message"] = "✅ Product added successfully.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ Error: " + ex.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            using var conn = OracleHelper.GetConnection();
            string query = "UPDATE SYSTEM.PRODUCTS SET ISACTIVE = 'No' WHERE PRODUCTID = :id";

            using var cmd = new OracleCommand(query, conn);
            cmd.Parameters.Add("id", id);
            cmd.ExecuteNonQuery();

            return RedirectToPage();
        }

        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = "";
            public string SKU { get; set; } = "";
            public decimal UnitPrice { get; set; }
            public int InStockQty { get; set; }
            public string Category { get; set; } = "";
        }

        public class Category
        {
            public string CategoryId { get; set; } = "";
            public string CategoryName { get; set; } = "";
        }
    }
}
