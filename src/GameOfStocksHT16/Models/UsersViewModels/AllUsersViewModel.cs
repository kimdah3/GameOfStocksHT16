using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GameOfStocksHT16.Models.UsersViewModels
{
    public class AllUsersViewModel
    {
        public List<User> AllUsers { get; set; }
        public List<ProfileViewModel> LeaderBoard { get; set; }
    }

    public class User
    {
        public string Email { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        public decimal Money { get; set; }
    }
}
