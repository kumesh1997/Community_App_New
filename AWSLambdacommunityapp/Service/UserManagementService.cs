using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Dto;
using AWSLambdacommunityapp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class UserManagementService
    {
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly AmazonDynamoDBClient _amazonDynamoDBClient;

        public UserManagementService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();

            _amazonDynamoDBClient = connectToDynamoDB.AmazonDynamoDBClient();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> VisitorFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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


            if (httpMethod == "POST" && request.Body != null && request.PathParameters == null)
            {
                return await HandleUserRegistrationRequest(request);
            }else if (httpMethod == "GET" && request.Body == null && request.PathParameters != null)
            {
                return await HandleGetUserDetailsRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Handle User Register
        /*private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUserRegistrationRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            var user = JsonSerializer.Deserialize<User>(request.Body);
            try
            {
                user.Email_Verified = true;
                user.Is_Super_Admin = false;
                await _dynamoDbContext.SaveAsync(user);
                return OkResponse();
            }
            catch (Exception ex)
            {
                return BadResponse("User was not registered !!! "+ ex.Message);
            }
            
        }*/

        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUserRegistrationRequest(
          APIGatewayHttpApiV2ProxyRequest request)
        {
            var userDto = JsonSerializer.Deserialize<UserRegisterDto>(request.Body);
            var document = new Document();
            try
            {
                
                    document["UserId"] = userDto.UserId;
                    document["Password"] = userDto.Password;
                    document["FullName"] = userDto.FullName;
                    document["NIC"] = userDto.NIC;
                    document["Tower"] = userDto.Tower;
                    document["Floor"] = userDto.Floor;
                    document["House_Number"] = userDto.House_Number;
                    document["Is_Super_Admin"] = false;
                    document["Phone_Number"] = userDto.Phone_Number;
                    document["Email_Verified"] = true;

                var table = Table.LoadTable(_amazonDynamoDBClient, "User");
                var res = table.PutItemAsync(document);
                if (res != null)
                {
                    return OkResponse();
                }
                return BadResponse("User was not registered !!!");
            }
            catch (Exception ex)
            {
                return BadResponse("User was not registered !!! " + ex.Message);
            }
        }

        // Get User Details
        /*private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetUserDetailsRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // get the parameter value
                request.PathParameters.TryGetValue("Id", out var Id);
                // Get Users
                var users = await _dynamoDbContext.ScanAsync<User>(default).GetRemainingAsync();
                var Selected_User = users.Where(v => v.UserId.Equals(Id));
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(Selected_User),
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return BadResponse(" Exception " + ex.Message);
            }
        }*/


        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetUserDetailsRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // get the parameter value
                request.PathParameters.TryGetValue("Id", out var Id);
                // Get Users
                var table = Table.LoadTable(_amazonDynamoDBClient, "User");
                var search = await table.GetItemAsync(Id);
                if (search != null)
                {
                    var user = new User
                    {
                        UserId = search["UserId"].AsString(),
                        // Add other static properties here

                        // Retrieve dynamic attributes as a dictionary
                        /*AdditionalAttributes = search.ToDictionary()
                       .Where(kvp => !string.Equals(kvp.Key, "UserId", StringComparison.OrdinalIgnoreCase)) // Exclude UserId from dynamic attributes
                       .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsString()) // Convert all values to string for simplicity*/

                    };
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = JsonSerializer.Serialize(user),
                        StatusCode = 200
                    };
                }
                return BadResponse(" User was not Found !!! ");
            }
            catch (Exception ex)
            {
                return BadResponse(" Exception " + ex.Message);
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
