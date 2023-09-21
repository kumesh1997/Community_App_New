using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class AmenityUser
    {
        public string AmenityBookingUserId { get; set; }
        //public string AmenityBookingUserNIC { get; set; }
        public int NumberofBookingByTheUser { get; set; }

        public int StartTime { get; set; }

        public AmenityUser(string Id, int Count, int time) { 
            AmenityBookingUserId = Id;
            NumberofBookingByTheUser = Count;
            StartTime = time;
        }
    }
}
