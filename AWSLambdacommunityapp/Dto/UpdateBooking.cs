using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class UpdateBooking
    {
        public string Booking_Id { get; set; }
        public string Status { get; set; }
    }
}
