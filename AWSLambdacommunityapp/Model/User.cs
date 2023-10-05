using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
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

        public Dictionary<string, string> AdditionalAttributes = new Dictionary<string, string>();
    }
}
