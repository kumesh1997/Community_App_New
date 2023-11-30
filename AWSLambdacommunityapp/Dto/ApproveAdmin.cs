using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class ApproveAdmin
    {
        public string UserId { get; set; }
        public string Module_Id { get; set; }
        public bool Is_Edit { get; set; }
        //public Dictionary<string, bool>[] policyList { get; set; }
    }
}
