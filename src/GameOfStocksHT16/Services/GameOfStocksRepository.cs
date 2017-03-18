using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameOfStocksHT16.Services
{
    public class GameOfStocksRepository : IGameOfStocksRepository
    {
        private readonly ApplicationDbContext _context;

        public GameOfStocksRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<StockOwnership> GetStockOwnershipsByUser(ApplicationUser user)
        {
            return _context.StockOwnership.Where(x => x.User.Id == user.Id).ToList();
        }

        public void AddStockTransactions(StockTransaction stockTransaction)
        {
            _context.Add(stockTransaction);
        }

        public ApplicationUser GetUserById(string userId)
        {
            return _context.Users.Include(x => x.StockTransactions).Include(x => x.StockOwnerships).Include(x => x.MoneyHistory).FirstOrDefault(u => u.Id == userId);
        }

        public ApplicationUser GetUserByEmail(string email)
        {
            return _context.Users.Include(x => x.StockTransactions).Include(x => x.StockOwnerships).Include(x => x.MoneyHistory).FirstOrDefault(u => u.Email == email);
        }

        public IEnumerable<StockTransaction> GetStockTransactionsByUser(ApplicationUser user)
        {
            return _context.StockTransaction.Where(x => x.User.Id == user.Id).ToList();
        }

        public IEnumerable<StockTransaction> GetUncompletedStockTransactions()
        {
            return _context.StockTransaction.Include(x => x.User).Where(x => !x.IsCompleted).ToList();
        }

        public void AddStockOwnerships(List<StockOwnership> newOwnerships)
        {
            _context.StockOwnership.AddRange(newOwnerships);
        }

        private bool StockTransactionExists(int id)
        {
            return _context.StockTransaction.Any(e => e.Id == id);
        }

        public StockOwnership GetStockOwnershipByUserAndLabel(ApplicationUser user, string label)
        {
            return _context.StockOwnership.FirstOrDefault(s => s.User == user && s.Label == label);
        }

        public IEnumerable<ApplicationUser> GetAllUsers()
        {
            return _context.Users.Include(u => u.StockOwnerships).Include(u => u.StockTransactions).Include(u => u.MoneyHistory).ToList();
        }

        public IEnumerable<ApplicationUser> GetUsersWithPendingStockTransactions()
        {
            return _context.Users.Include(u => u.StockTransactions).Where(u => u.StockTransactions.Any(s => (s.IsBuying || s.IsSelling) && !s.IsCompleted && !s.IsFailed)).ToList();
        }

        public bool UsersExists()
        {
            return _context.Users.Any();
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }

        public IEnumerable<UserMoneyHistory> GetAllUsersTotalYesterday()
        {
            return _context.UserMoneyHistory.Include(x => x.User).Where(x => x.Time == DateTime.Today).ToList();
        }


        public void SaveUsersHistory(List<UserMoneyHistory> list)
        {
            foreach (var entity in list)
            {
                _context.UserMoneyHistory.Add(entity);
            }
            Save();
        }
        public void RemoveStockOwnership(StockOwnership stockOwnership)
        {
            _context.StockOwnership.Remove(stockOwnership);
        }

        public bool StockTransactionExists(StockTransaction stockTransaction)
        {
            return _context.StockTransaction.Any(e => e.Id == stockTransaction.Id);
        }

        public StockTransaction GetStockTransactionById(int id)
        {
            return _context.StockTransaction.SingleOrDefault(m => m.Id == id);
        }

        public IEnumerable<StockTransaction> GetSellingStockTransactionsByUser(ApplicationUser user)
        {
            return _context.StockTransaction.Where(x => x.User == user && x.IsSelling && !x.IsCompleted).ToList();
        }

        public IEnumerable<UserMoneyHistory> GetUserMoneyHistory(ApplicationUser user)
        {
            return _context.UserMoneyHistory.Where(x => x.User == user).ToList();
        }

        public IEnumerable<UserMoneyHistory> GetAllUserMoneyHistory()
        {
            return _context.UserMoneyHistory.ToList();
        }

        public UserMoneyHistory GetUserTotalYesterdayByUser(ApplicationUser user)
        {
            return _context.UserMoneyHistory.FirstOrDefault(x => x.User == user && x.Time == DateTime.Today);
        }

        public IEnumerable<StockTransaction> GetCompletedStockTransactionsSortedByDate()
        {
            var transactions = _context.StockTransaction.Include(s => s.User).Where(s => !s.IsFailed && s.IsCompleted).OrderByDescending(x => x.Date).Take(100).ToList();
            return transactions;
        }

        public IEnumerable<ApplicationUser> GetSearchResult(string name)
        {
            var result = _context.Users.Where(x => x.FullName.Contains(name)).ToList();
            return result;
        }
    }
}
