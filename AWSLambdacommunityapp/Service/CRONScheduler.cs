using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class CRONScheduler
    {
        // Reference to DynamoDBContext
        private readonly DynamoDBContext _dynamoDbContext;


        public CRONScheduler()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();
        }


        public async Task<APIGatewayHttpApiV2ProxyResponse> CRONSchedulerFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            try
            {
                string[] DateArray = { "04:30PM", "05:00PM", "05:30PM", "06:00PM", "06:30PM", "07:00PM", "07:30PM", "08:00PM", "08:30PM", "09:00PM", "09:30PM", "10:00PM", "10:30PM", "11:00PM", "11:30PM", "00:00AM" };
                // Get Vailable Amenity List
                var AmenityList_List = await _dynamoDbContext.ScanAsync<Amenities>(default).GetRemainingAsync();
                foreach (var amenity in AmenityList_List)
                {
                    foreach (var item in DateArray)
                    {
                        CRON cRON = new CRON();
                        cRON.Id = GenerateId();
                        cRON.AmenityId = amenity.Id;
                        cRON.AmenityName = amenity.AmenityType;
                        cRON.Capacity = amenity.MaximumCapacityCount;
                        cRON.NumberOfUsers = 0;
                        cRON.CondoId = amenity.Condo_ID;
                        cRON.TimeSlot = item;
                        cRON.Date = GetCurrentDateInYYMMDD();

                        await _dynamoDbContext.SaveAsync(cRON);
                    }
                }
                return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 200 };
            }
            catch (Exception ex)
            {
                // Handle unsupported or unrecognized HTTP methods
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = ex.Message,
                    StatusCode = 400
                };
            }
        }

        public static string GetCurrentDateInYYMMDD()
        {
            DateTime currentDate = DateTime.UtcNow; // Use UtcNow for consistent results

            // Format the date as yymmdd
            string formattedDate = currentDate.ToString("yyMMdd");

            return formattedDate;
        }

        // Autogenerate ID
        public string GenerateId()
        {
            Guid guid = Guid.NewGuid();
            string id = guid.ToString();
            return id;
        }
    }
}
