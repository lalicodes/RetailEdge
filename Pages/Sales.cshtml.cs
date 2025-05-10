using Microsoft.AspNetCore.Mvc.RazorPages;
using Oracle.ManagedDataAccess.Client;
using RetailEdge.Web.DataAccess;

namespace RetailEdge.Web.Pages
{
    public class SalesModel : PageModel
    {
        public List<Order> Orders { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AvgOrderValue { get; set; }

        public void OnGet()
        {
            using var conn = OracleHelper.GetConnection();
            string query = @"
                SELECT O.ORDERID, O.CUSTOMER_NAME, O.ORDER_DATE, O.STATUS, O.TOTAL_AMOUNT,
                       I.ORDER_ITEM_ID, I.PRODUCT_ID, P.PRODUCTNAME, I.QUANTITY, I.UNIT_PRICE
                FROM SYSTEM.ORDERS O
                LEFT JOIN SYSTEM.ORDER_ITEMS I ON O.ORDERID = I.ORDER_ID
                LEFT JOIN SYSTEM.PRODUCTS P ON I.PRODUCT_ID = P.PRODUCTID
                ORDER BY O.ORDER_DATE DESC, O.ORDERID";

            using var cmd = new OracleCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            var orderMap = new Dictionary<int, Order>();
            while (reader.Read())
            {
                int orderId = reader.GetInt32(0);
                if (!orderMap.ContainsKey(orderId))
                {
                    orderMap[orderId] = new Order
                    {
                        OrderId = orderId,
                        CustomerName = reader.GetString(1),
                        OrderDate = reader.GetDateTime(2),
                        Status = reader.GetString(3),
                        TotalAmount = reader.GetDecimal(4),
                        Items = new List<OrderItem>()
                    };
                }

                if (!reader.IsDBNull(5))
                {
                    orderMap[orderId].Items.Add(new OrderItem
                    {
                        ItemId = reader.GetInt32(5),
                        ProductId = reader.GetInt32(6),
                        ProductName = reader.IsDBNull(7) ? "(Unknown)" : reader.GetString(7),
                        Quantity = reader.GetInt32(8),
                        UnitPrice = reader.GetDecimal(9)
                    });
                }
            }

            Orders = orderMap.Values.ToList();
            TotalOrders = Orders.Count;
            TotalRevenue = Orders.Sum(o => o.TotalAmount);
            AvgOrderValue = TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;
        }

        public class Order
        {
            public int OrderId { get; set; }
            public string CustomerName { get; set; } = "";
            public DateTime OrderDate { get; set; }
            public string Status { get; set; } = "";
            public decimal TotalAmount { get; set; }
            public List<OrderItem> Items { get; set; } = new();
        }

        public class OrderItem
        {
            public int ItemId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}
