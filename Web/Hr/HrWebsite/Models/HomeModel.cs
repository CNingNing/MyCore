using System;
using DBModels.Hr;
namespace HrWebsite.Models
{
    public class HomeModel
    {
       public long Id { get; set; }

        public User User { get; set; }

        public  UserRole UserRole { get; set; }
    }
}