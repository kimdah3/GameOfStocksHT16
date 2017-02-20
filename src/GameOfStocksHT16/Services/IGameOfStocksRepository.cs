using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Entities;

namespace GameOfStocksHT16.Services
{
    public interface IGameOfStocksRepository
    {
        void AddStockTransactions(StockTransaction stockTransaction);
        ApplicationUser GetUserById(string userId);
        IEnumerable<StockTransaction> GetUncompletedStockTransactions();
        void AddStockOwnerships(List<StockOwnership> newOwnerships);

        bool Save();

        StockOwnership GetStockOwnershipByUserAndLabel(ApplicationUser user, string label);
        IEnumerable<ApplicationUser> GetAllUsers();
        void RemoveStockOwnership(StockOwnership stockOwnership);

        bool StockTransactionExists(StockTransaction stockTransaction);
        StockTransaction GetStockTransactionById(int id);
    }
}
