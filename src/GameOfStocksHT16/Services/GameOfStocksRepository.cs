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
        private ApplicationDbContext _context;

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
            return _context.Users.FirstOrDefault(u => u.Id == userId);
        }

        public ApplicationUser GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public IEnumerable<StockTransaction> GetStockTransactionsByUser(ApplicationUser user)
        {
            return _context.StockTransaction.Where(x => x.User.Id == user.Id).ToList();
        }

        public IEnumerable<StockTransaction> GetUncompletedStockTransactions()
        {
            return _context.StockTransaction.Include(x => x.User).Where(x => !x.IsCompleted);
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
            return _context.Users.Include(u => u.StockOwnerships).ToList();
        }

        public List<ApplicationUser> GetUsersWithPendingStockTransactions()
        {
            //var users = _context.Users.Include(u => u.StockTransactions);
            //var usersWithPendingStockTransactions = new List<ApplicationUser>();
            //foreach (var user in users)
            //{
            //    var hasPending = false;
            //    foreach (var transaction in user.StockTransactions)
            //    {
            //        if (transaction.IsBuying || transaction.IsSelling)
            //        {
            //            hasPending = true;
            //        }
            //    }
            //    if(hasPending)
            //        usersWithPendingStockTransactions.Add(user);
            //}
            return _context.Users.Where(u => u.StockTransactions.TrueForAll(s => s.IsBuying || s.IsSelling)).ToList();
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
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

        public List<StockTransaction> GetSellingStockTransactionsByUser(ApplicationUser user)
        {
            return _context.StockTransaction.Where(x => x.User == user && x.IsSelling && !x.IsCompleted).ToList();
        }
    }
}
