using Amazon.DynamoDBv2.DataModel;
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
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using Amazon.CognitoIdentityProvider;
using Amazon;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using Amazon.DynamoDBv2.Model;

namespace AWSLambdacommunityapp.Service
{
    public class AdminSevice
    {
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly AmazonDynamoDBClient _amazonDynamoDBClient;

        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly string _userPoolId = "us-east-1_dRHFkZCMr";
        private readonly string _clientId = "7drftsbsv2tm316d72o6ek6422";
        private readonly string _clientSecret = "1q807qber9cpm1blu782jndeo6fg7mck6ncrpdbblq9c31bib6jk";
        private readonly RegionEndpoint _region = RegionEndpoint.USEast1;

        public AdminSevice()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();
            _amazonDynamoDBClient = connectToDynamoDB.AmazonDynamoDBClient();

            var region = RegionEndpoint.USEast1; // e.g., RegionEndpoint.USWest2
            _cognitoClient = new AmazonCognitoIdentityProviderClient(region);
        }


        public async Task<APIGatewayHttpApiV2ProxyResponse> AdminFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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
                return await HandleAdminRegistrationRequest(request);
            }
            else if (httpMethod == "PUT" && request.Body != null && request.PathParameters == null)
            {
                return await HandleUpdateAdminRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Handle Admin Register
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleAdminRegistrationRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            var userDto = JsonSerializer.Deserialize<UserDto>(request.Body);
            var document = new Document();
            try
            {
                if (userDto != null || userDto.Email_Verified != false)
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
                    if (userDto.policyList != null)
                    {
                        document["UserId"] = userDto.UserId;
                        document["Password"] = userDto.Password;
                        document["FullName"] = userDto.FullName;
                        document["NIC"] = userDto.NIC;
                        document["Tower"] = userDto.Tower;
                        document["Floor"] = userDto.Floor;
                        document["House_Number"] = userDto.House_Number;
                        document["Is_Super_Admin"] = userDto.Is_Super_Admin;
                        document["Is_Admin"] = userDto.Is_Admin;
                        document["Is_Condo_Admin"] = userDto.Is_Condo_Admin;
                        document["Phone_Number"] = userDto.Phone_Number;
                        document["Email_Verified"] = true;
                        document["Is_Editable"] = userDto.Is_Editable? true : false;
                        document["Condominum_Id"] = userDto.Condominium_ID;
                        document["Module_ID"] = userDto.Module_ID;

                        foreach (var policy in userDto.policyList)
                            {
                                foreach (var kvp in policy)
                                {
                                    document[kvp.Key] = kvp.Value;
                                }
                            }

                    }
                    //await _dynamoDbContext.SaveAsync(document);
                    var table = Table.LoadTable(_amazonDynamoDBClient, "User");
                    table.PutItemAsync(document);
                    return OkResponse();
                }
                else
                {
                    return BadResponse(" User is not verified !!!!");
                }
            }
            catch (Exception ex)
            {
                return BadResponse("User was not registered !!! " + ex.Message);
            }

        }

        // Handle Update Admin Policies
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUpdateAdminRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var updatedUser = System.Text.Json.JsonSerializer.Deserialize<ApproveAdmin>(request.Body);
                if (string.IsNullOrWhiteSpace(updatedUser.UserId))
                {
                    return BadResponse("Invalid request. 'UserId' is required in the request body.");
                }

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
                        UpdateExpression = "SET Module_ID = :Module_ID, Is_Editable = :Is_Editable",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":Module_ID", new AttributeValue { S = updatedUser.Module_Id } },
                        { ":Is_Editable", new AttributeValue {  N = updatedUser.Is_Edit? "1" : "0" } }
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
                    return BadResponse("User was Not Updated !!! ");
                }

            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "Error: " + ex.Message,
                    StatusCode = 500
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
