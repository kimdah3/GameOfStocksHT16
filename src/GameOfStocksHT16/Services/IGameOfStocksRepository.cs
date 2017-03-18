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
        IEnumerable<ApplicationUser> GetUsersWithPendingStockTransactions();

        StockTransaction GetStockTransactionById(int id);
        IEnumerable<StockTransaction> GetStockTransactionsByUser(ApplicationUser user);
        IEnumerable<StockTransaction> GetUncompletedStockTransactions();
        IEnumerable<StockTransaction> GetSellingStockTransactionsByUser(ApplicationUser user);

        StockOwnership GetStockOwnershipByUserAndLabel(ApplicationUser user, string label);
        IEnumerable<StockOwnership> GetStockOwnershipsByUser(ApplicationUser user);

        IEnumerable<UserMoneyHistory> GetUserMoneyHistory(ApplicationUser user);
        UserMoneyHistory GetUserTotalYesterdayByUser(ApplicationUser user);
        IEnumerable<StockTransaction> GetCompletedStockTransactionsSortedByDate();

        void SaveUsersHistory(List<UserMoneyHistory> list);

        void AddStockTransactions(StockTransaction stockTransaction);
        void RemoveStockOwnership(StockOwnership stockOwnership);
        void AddStockOwnerships(List<StockOwnership> newOwnerships);

        bool StockTransactionExists(StockTransaction stockTransaction);
        bool UsersExists();
        bool Save();

        IEnumerable<UserMoneyHistory> GetAllUsersTotalYesterday();
        IEnumerable<ApplicationUser> GetSearchResult(string name);
    }
}
