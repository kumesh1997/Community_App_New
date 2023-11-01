using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class Condominium
    {
        public string Condominium_Id { get; set; }
        public string Condominium_Description { get; set; }
        public string Condo_Address { get; set; }
        public string Contact { get; set; }
        public bool Is_Delete { get; set; } 
    }
}
