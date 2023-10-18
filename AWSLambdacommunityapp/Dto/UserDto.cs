using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string NIC { get; set; }
        public string Tower { get; set; }
        public string Floor { get; set; }
        public string House_Number { get; set; }
        public bool Is_Super_Admin { get; set; }
        public string Phone_Number { get; set; }
        public bool Email_Verified { get; set; }

        public string Condominium_ID { get; set; }

        //public List<Dictionary<string, bool>> policyList { get; set; }
        public Dictionary<string, bool>[] policyList { get; set; }
    }
}
