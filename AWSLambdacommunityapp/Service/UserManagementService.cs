﻿using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
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

        public UserManagementService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();
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
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUserRegistrationRequest(
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
            
        }

        // Get User Details
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetUserDetailsRequest(APIGatewayHttpApiV2ProxyRequest request)
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
