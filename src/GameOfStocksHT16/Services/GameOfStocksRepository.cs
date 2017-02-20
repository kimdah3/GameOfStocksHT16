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

        public void AddStockTransactions(StockTransaction stockTransaction)
        {
            _context.Add(stockTransaction);



        }

        public void UpdateUser(ApplicationUser user)
        {
            _context.Users.Update(user);
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        public ApplicationUser GetUserById(string userId)
        {
            return _context.Users.FirstOrDefault(u => u.Id == userId);
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

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }

        public StockOwnership GetExistingStockByUserAndLabel(ApplicationUser user, string label)
        {
            return _context.StockOwnership.FirstOrDefault(s => s.User == user && s.Label == label);
        }

        public IEnumerable<ApplicationUser> GetAllUsers()
        {
            return _context.Users.Include(u => u.StockOwnerships).ToList();
        }
    }
}
