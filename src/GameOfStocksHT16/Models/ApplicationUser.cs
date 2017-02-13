using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GameOfStocksHT16.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public decimal Money { get; set; }
        public decimal PendingMoney { get; set; }

        public virtual List<StockTransaction> StockTransactions { get; set; }
    }
}
