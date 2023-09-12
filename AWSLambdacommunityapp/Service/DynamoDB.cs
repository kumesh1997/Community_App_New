using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class DynamoDB
    {
        public DynamoDBContext DBAccessFunction()
        {
            return new DynamoDBContext(new AmazonDynamoDBClient());
        }
    }
}
