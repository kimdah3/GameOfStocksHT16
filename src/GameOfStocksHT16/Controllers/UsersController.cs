using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Models.UsersViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace GameOfStocksHT16.Controllers
{
    public class UsersController : Controller
    {
        public ApplicationDbContext DbContext { get; set; }

        public UsersController(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        // GET: Users
        public ActionResult Index()
        {
            var users = DbContext.Users.ToList();
            var model = new AllUsersViewModel() { AllUsers = new List<User>()};
            users.ForEach(x => model.AllUsers.Add(new User() {Email = x.Email, Money = x.Money}));
            return View(model);
        }
        
    }
}