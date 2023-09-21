using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class AmenityBooking
    {
        public string Id { get; set; }
        public string AmenityTypeId { get; set; }
        public List<AmenityUser> AttendeesList { get; set; } 
        public int BookingCount { get; set; }
        public bool IsFull { get; set; } = false;
        // 30 Mins Slots
        public int Time { get; set; }
    }
}
