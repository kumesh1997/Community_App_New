using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class Event
    {
        // Events date , time 
        public string Id { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public int DateTime { get; set; }
        public bool IsVisible { get; set; }

        public string Image { get; set; }

    }
}
