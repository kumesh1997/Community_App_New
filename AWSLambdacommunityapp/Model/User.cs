using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
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

        //[DynamoDBProperty("Attributes")]
        //public Dictionary<string, AttributeValue> Attributes { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string NIC { get; set; }
        public string Tower { get; set; }
        public string Floor { get; set; }
        public string House_Number { get; set; }
        public bool Is_Super_Admin { get; set; }
        public string Phone_Number { get; set; }
        public bool Email_Verified { get; set; }
        public bool Is_Admin { get; set; }
        public bool Is_Condo_Admin { get; set; }
        public bool Is_Editable { get; set; }
        public string Condominum_Id { get; set; }
    }

}
