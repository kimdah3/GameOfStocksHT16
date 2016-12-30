using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameOfStocksHT16.Data;
using GameOfStocksHT16.Models;
using GameOfStocksHT16.Models.UsersViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace GameOfStocksHT16.Controllers
{
    public class UsersController : Controller
    {
        public ApplicationDbContext DbContext { get; set; }
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            DbContext = dbContext;
            _userManager = userManager;
        }

        // GET: Users
        public ActionResult Index()
        {
            var users = DbContext.Users.ToList();
            var model = new AllUsersViewModel() { AllUsers = new List<User>() };
            users.ForEach(x => model.AllUsers.Add(new User() { Email = x.Email, Money = x.Money }));
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult> DisplayProfile()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user != null)
            {
                var model = new DisplayProfileViewModel()
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Money = user.Money,
                    StockTransactions = DbContext.StockTransaction.Where(x => x.User.Id == user.Id).ToList(),
                    StockOwnerships = DbContext.StockOwnership.Where(x => x.User.Id == user.Id).ToList()
                };
                return View(model);
            }
            return RedirectToAction("Login","Account");
        }
    }
}