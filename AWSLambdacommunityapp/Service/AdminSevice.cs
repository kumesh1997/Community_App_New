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

namespace AWSLambdacommunityapp.Service
{
    public class AdminSevice
    {
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly AmazonDynamoDBClient _amazonDynamoDBClient;

        public AdminSevice()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();
            _amazonDynamoDBClient = connectToDynamoDB.AmazonDynamoDBClient();
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
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters != null)
            {
                //return await HandleGetUserDetailsRequest(request);
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
                    if (userDto.Is_Super_Admin == true && userDto.policyList != null)
                    {
                        /*user.UserId = userDto.UserId;
                        user.Password = userDto.Password;
                        user.Name = userDto.Name;
                        user.Is_Super_Admin = true;
                        user.Email_Verified = true;
                        user.Phone_Number = userDto.Phone_Number;*/
                        document["UserId"] = userDto.UserId;
                        document["Password"] = userDto.Password;
                        document["FullName"] = userDto.FullName;
                        document["NIC"] = userDto.NIC;
                        document["Tower"] = userDto.Tower;
                        document["Floor"] = userDto.Floor;
                        document["House_Number"] = userDto.House_Number;
                        document["Is_Super_Admin"] = true;
                        document["Phone_Number"] = userDto.Phone_Number;
                        document["Email_Verified"] = true;
                        
                            foreach (var policy in userDto.policyList)
                            {
                                foreach (var kvp in policy)
                                {
                                    document[kvp.Key] = kvp.Value;
                                }
                            }

                    }else if (userDto.policyList != null && userDto.Is_Super_Admin != true)
                    {
                        /* user.UserId = userDto.UserId;
                         user.Password = userDto.Password;
                         user.Name = userDto.Name;
                         user.Is_Super_Admin = false;
                         user.Email_Verified = true;
                         user.Phone_Number = userDto.Phone_Number;
                         if (policyList != null)
                         {
                             foreach (var policyItem in policyList)
                             {
                                 foreach (var kvp in policyItem)
                                 {
                                     user.AdditionalAttributes.Add(kvp.Key, kvp.Value);
                                 }
                             }
                         }*/
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
                        if (userDto.policyList != null)
                        {
                            foreach (var policy in userDto.policyList)
                            {
                                foreach (var kvp in policy)
                                {
                                    document[kvp.Key] = kvp.Value;
                                }
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
