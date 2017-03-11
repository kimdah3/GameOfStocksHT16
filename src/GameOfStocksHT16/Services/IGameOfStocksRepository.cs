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
        ApplicationUser GetUserById(string userId);
        ApplicationUser GetUserByEmail(string email);
        IEnumerable<ApplicationUser> GetAllUsers();
        List<ApplicationUser> GetUsersWithPendingStockTransactions();

        StockTransaction GetStockTransactionById(int id);
        IEnumerable<StockTransaction> GetStockTransactionsByUser(ApplicationUser user);
        IEnumerable<StockTransaction> GetUncompletedStockTransactions();
        List<StockTransaction> GetSellingStockTransactionsByUser(ApplicationUser user);

        StockOwnership GetStockOwnershipByUserAndLabel(ApplicationUser user, string label);
        IEnumerable<StockOwnership> GetStockOwnershipsByUser(ApplicationUser user);

        List<UserMoneyHistory> GetUserMoneyHistory(ApplicationUser user);
        List<StockTransaction> GetCompletedStockTransactionsSortedByDate();

        void SaveUsersHistory(List<UserMoneyHistory> list);

        void AddStockTransactions(StockTransaction stockTransaction);
        void RemoveStockOwnership(StockOwnership stockOwnership);
        void AddStockOwnerships(List<StockOwnership> newOwnerships);

        bool StockTransactionExists(StockTransaction stockTransaction);
        bool UsersExists();
        bool Save();

        List<UserMoneyHistory> GetAllUsersTotalYesterday();
    }
}
