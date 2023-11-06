using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class EventDto
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string ImageName { get; set; }
        public string Image64Base { get; set; }
        public string Location { get; set; }
    }
}
