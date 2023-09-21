using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class User
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public bool Is_Super_Admin { get; set; } = false;
        public string Phone_Number { get; set; }
        public bool Email_Verified { get; set; } = false;

        //Condominium_Id, Module_Id
    }
}
