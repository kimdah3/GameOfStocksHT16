using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.UsersViewModels
{
    public class AllUsersViewModel
    {
        public List<User> AllUsers { get; set; }
    }

    public class User
    {
        public string Email { get; set; }
        public decimal Money { get; set; }
    }
}
