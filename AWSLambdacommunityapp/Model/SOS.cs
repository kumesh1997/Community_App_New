using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class SOS
    {
        //TIME,user, comment, status(open, inprogerss, close))
        public string Id { get; set; }
        public int SOSTime { get; set; }
        public string UserId { get; set; }
        public string Comment { get; set; }
        public int Updated_Time { get; set; }
        public string Status { get; set; }

    }
}
