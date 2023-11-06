using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using AWSLambdacommunityapp.Dto;
using AWSLambdacommunityapp.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class UserManagementService
    {
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly AmazonDynamoDBClient _amazonDynamoDBClient;

        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly string _userPoolId = "us-east-1_dRHFkZCMr";
        private readonly string _clientId = "7drftsbsv2tm316d72o6ek6422";
        private readonly string _clientSecret = "1q807qber9cpm1blu782jndeo6fg7mck6ncrpdbblq9c31bib6jk";
        private readonly RegionEndpoint _region = RegionEndpoint.USEast1;


        public UserManagementService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();

            _amazonDynamoDBClient = connectToDynamoDB.AmazonDynamoDBClient();

            var region = RegionEndpoint.USEast1; // e.g., RegionEndpoint.USWest2
            _cognitoClient = new AmazonCognitoIdentityProviderClient(region);
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
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters == null)
            {
                return await HandleGetUserListDetailsRequest(request);
            }
            else if (httpMethod == "PUT" && request.Body != null && request.PathParameters == null)
            {
                return await HandleApproveRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Register User
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUserRegistrationRequest(
          APIGatewayHttpApiV2ProxyRequest request)
        {
            var userDto = System.Text.Json.JsonSerializer.Deserialize<UserRegisterDto>(request.Body);
            var document = new Document();
            try
            {
                var provider = new AmazonCognitoIdentityProviderClient(
                new BasicAWSCredentials(_clientId, _clientSecret), _region);
                
                // If there us a client secret, it should also be added here brfor provider
                var pool = new CognitoUserPool(_userPoolId, _clientId, provider);
               var userAttributes = new Dictionary<string, string>
                {
                    { "email", userDto.UserId },
                    // Add other user attributes as needed
                };

                try
                {
                    await pool.SignUpAsync(userDto.UserId, userDto.Password, userAttributes, null);
                    Console.WriteLine("User registered successfully.");
                }
                catch (Exception ex)
                {
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = ex.Message,
                        StatusCode = 400
                    };
                }
                    // Save User in DynamoDB
                    document["UserId"] = userDto.UserId;
                    document["Password"] = userDto.Password;
                    document["FullName"] = userDto.FullName;
                    document["NIC"] = userDto.NIC;
                    document["Tower"] = userDto.Tower;
                    document["Floor"] = userDto.Floor;
                    document["House_Number"] = userDto.House_Number;
                    document["Is_Super_Admin"] = false;
                    document["Phone_Number"] = userDto.Phone_Number;
                    document["Email_Verified"] = false;
                    document["Is_Admin"] = false;
                    document["Condominum_Id"] = userDto.Condominium_ID;

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


        // Get User Details of a Specific User
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
                    // Create a new JSON object in the desired format
                    var transformedData = new JObject();

                    foreach (var property in search)
                    {
                        var propertyName = property.Key;
                        var propertyValue = property.Value.ToString();

                        transformedData[propertyName] = propertyValue;
                    }

                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Body = JsonConvert.SerializeObject(transformedData),
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

        // Get a List of Users
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetUserListDetailsRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // Define a scan filter to find items where the "Email_Verified" attribute is false
                var scanFilter = new ScanFilter();
                scanFilter.AddCondition("Email_Verified", ScanOperator.Equal, false);

                // Perform the scan operation with the filter
                var userList = await _dynamoDbContext.FromScanAsync<User>(new ScanOperationConfig
                {
                    //Filter = scanFilter
                }).GetRemainingAsync();

                if (userList != null && userList.Count > 0)
                {
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = System.Text.Json.JsonSerializer.Serialize(userList),
                        StatusCode = 200
                    };
                }
                else
                {
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = "No unverified users found.",
                        StatusCode = 404
                    };
                }
            }
            catch(Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "Error: " + ex.Message,
                    StatusCode = 500
                };
            }
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Users are not Available !!!",
                StatusCode = 400
            };
        }

        // Approve a User, It can be an Admin or a User
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleApproveRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var updatedUser = System.Text.Json.JsonSerializer.Deserialize<ApproveUser>(request.Body);
                // Get Users
                var table = Table.LoadTable(_amazonDynamoDBClient, "User");
                var user = await table.GetItemAsync(updatedUser.UserId);
                if (user != null)
                {
                    // Create an UpdateItemRequest to specify the update
                    var updateRequest = new UpdateItemRequest
                    {
                        TableName = "User",
                        Key = new Dictionary<string, AttributeValue>
                    {
                        { "UserId", new AttributeValue { S = updatedUser.UserId } }
                    },
                        UpdateExpression = "SET Tower = :Tower, Floor = :Floor, House_Number = :House_Number, Email_Verified = :Email_Verified",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":Tower", new AttributeValue { S = updatedUser.Tower } },
                        { ":Floor", new AttributeValue { S = updatedUser.Floor } },
                        { ":House_Number", new AttributeValue { S = updatedUser.House_Number } },
                        { ":Email_Verified", new AttributeValue {  N = "1" } }
                    }
                     };

                    // Perform the update operation
                    await _amazonDynamoDBClient.UpdateItemAsync(updateRequest);

                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 200,
                        Body = "User updated successfully."
                    };
                }
                else
                {
                    return BadResponse("Invalid request, User was no Updated !!!");
                }
            }
            catch(Exception ex)
            {
                return BadResponse(ex.Message);
            }
        }

        // Update User

            private string CalculateSecretHash(string clientId, string clientSecret, string username)
        {
            var secret = $"{clientId}{username}{clientSecret}";
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(clientSecret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(secret));
                return Convert.ToBase64String(hashBytes);
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
