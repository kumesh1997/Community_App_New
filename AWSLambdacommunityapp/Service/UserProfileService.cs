using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class UserProfileService
    {
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly AmazonDynamoDBClient _amazonDynamoDBClient;

        public UserProfileService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();

            _amazonDynamoDBClient = connectToDynamoDB.AmazonDynamoDBClient();
        }



        public async Task<APIGatewayHttpApiV2ProxyResponse> UserProfileFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            string httpMethod = request.RequestContext.Http.Method.ToUpper();

            if (httpMethod == "OPTIONS")
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 200,
                    Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "http://localhost:3000" },
                    { "Access-Control-Allow-Headers", "Content-Type" },
                    { "Access-Control-Allow-Methods", "OPTIONS,POST,GET" },
                    { "Access-Control-Allow-Credentials", "true" },
                },
                };
            }


            if (httpMethod == "PUT" && request.Body != null && request.PathParameters == null)
            {
                return await HandleUpdateProfileRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Register User
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUpdateProfileRequest(
          APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var updatedUser = System.Text.Json.JsonSerializer.Deserialize<UpdateProfileDto>(request.Body);
                // Get Users
                var table = Table.LoadTable(_amazonDynamoDBClient, "User");
                var user = await table.GetItemAsync(updatedUser.Id);
                if (user != null)
                {
                    if (updatedUser.FullName.Length != 0 && updatedUser.Phone_Number.Length != 0)
                    {
                        // Create an UpdateItemRequest to specify the update
                        var updateRequest = new UpdateItemRequest
                        {
                            TableName = "User",
                            Key = new Dictionary<string, AttributeValue>
                    {
                        { "UserId", new AttributeValue { S = updatedUser.Id } }
                    },
                            UpdateExpression = "SET FullName = :FullName, Phone_Number = :Phone_Number",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":FullName", new AttributeValue { S = updatedUser.FullName } },
                        { ":Phone_Number", new AttributeValue { S = updatedUser.Phone_Number } }
                    }
                        };
                        // Perform the update operation
                        await _amazonDynamoDBClient.UpdateItemAsync(updateRequest);
                    }
                    else if (updatedUser.FullName.Length != 0 && updatedUser.Phone_Number.Length == 0)
                    {
                        // Create an UpdateItemRequest to specify the update
                        var updateRequest = new UpdateItemRequest
                        {
                            TableName = "User",
                            Key = new Dictionary<string, AttributeValue>
                    {
                        { "UserId", new AttributeValue { S = updatedUser.Id } }
                    },
                            UpdateExpression = "SET FullName = :FullName",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":FullName", new AttributeValue { S = updatedUser.FullName } },
                        
                    }
                        };
                        // Perform the update operation
                        await _amazonDynamoDBClient.UpdateItemAsync(updateRequest);
                    }
                    else if (updatedUser.FullName.Length == 0 && updatedUser.Phone_Number.Length != 0)
                    {
                        // Create an UpdateItemRequest to specify the update
                        var updateRequest = new UpdateItemRequest
                        {
                            TableName = "User",
                            Key = new Dictionary<string, AttributeValue>
                    {
                        { "UserId", new AttributeValue { S = updatedUser.Id } }
                    },
                            UpdateExpression = "SET Phone_Number = :Phone_Number",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":Phone_Number", new AttributeValue { S = updatedUser.Phone_Number } }
                    }
                        };
                        // Perform the update operation
                        await _amazonDynamoDBClient.UpdateItemAsync(updateRequest);
                    }
                   
                    return OkResponse();
                }
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "User Not Found !!!",
                    StatusCode = 400
                };
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = ex.Message,
                    StatusCode = 400
                };
            }
        }

        // OK Response
        private static APIGatewayHttpApiV2ProxyResponse OkResponse() =>
            new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 200
            };

        // Bad Response
        private static APIGatewayHttpApiV2ProxyResponse BadResponse(string message)
        {
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = message,
                StatusCode = 404
            };
        }

    }
}
