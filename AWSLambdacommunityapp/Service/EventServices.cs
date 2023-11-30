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
    public class EventServices
    {
        // Reference to DynamoDBContext
        private readonly DynamoDBContext _dynamoDbContext;
        // Reference to S3Bucket
        private readonly S3BucketService _bucketService;
        // DynamoDB Client
        private readonly AmazonDynamoDBClient _amazonDynamoDBClient;

        public EventServices()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();

            // Instance of S3BucketService
            _bucketService = new S3BucketService();

            // Instance of DynamoDB Client
            _amazonDynamoDBClient = connectToDynamoDB.AmazonDynamoDBClient();

        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> EventFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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
                return await HandleAddEventsRequest(request);
            }
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters == null)
            {
                return await HandleGetEventsRequest(request);
            }
            else if (httpMethod == "PUT" && request.Body != null && request.PathParameters == null)
            {
                return await HandleUpdateEventRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Handle Add Event
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleAddEventsRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var ev = JsonSerializer.Deserialize<EventDto>(request.Body);
                Event newEvent = new Event();
                newEvent.Id = GenerateId();
                newEvent.Date = ev.Date;
                newEvent.Time = ev.Time;
                newEvent.EventName = ev.EventName;
                newEvent.EventDescription = ev.EventDescription;
                newEvent.Location = ev.Location;
                newEvent.DateTime = GetCurrentEpoch();
                newEvent.Image = _bucketService.UploadImageAndGetUrl(ev.Image64Base, ev.ImageName);
                if (newEvent == null)
                {
                    return BadResponse("Invalid Request !!!");
                }
                else if (newEvent.Image == null)
                {
                    return BadResponse("Image Size is Too Large !!!");
                }
                await _dynamoDbContext.SaveAsync(newEvent);
                return OkResponse();
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = ex.Message,
                    StatusCode = 400
                };
            }
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Event Not Added !!!",
                StatusCode = 400
            };
        }

        // Get List of Events
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetEventsRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var event_List = await _dynamoDbContext.ScanAsync<Event>(default).GetRemainingAsync();
                // Filter Booking Where Status is not Accepted
                var filteredList = event_List.Where(v => v.IsVisible != false).ToList();
                if (event_List != null && event_List.Count > 0)
                {
                    // Convert Pre-signed URL into 64 Base Image
                    foreach (var item in event_List)
                    {
                        string im;
                        try
                        {
                            im = await _bucketService.DownloadImageAsBase64Async(item.Image);
                        }
                        catch (Exception ex)
                        {
                            im = ex.Message;
                        }
                        item.Image = im;
                    }
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = System.Text.Json.JsonSerializer.Serialize(event_List),
                        StatusCode = 200
                    };
                }
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = ex.Message,
                    StatusCode = 400
                };
            }
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Events not Found !!! ",
                StatusCode = 400
            };
        }

        // Update Event
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUpdateEventRequest(
      APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var updatedEvent = System.Text.Json.JsonSerializer.Deserialize<UpdateEventDto>(request.Body);
                if (string.IsNullOrWhiteSpace(updatedEvent.EventId))
                {
                    return BadResponse("Invalid request. 'EventId' is required in the request body.");
                }

                    var existingEvent = await _dynamoDbContext.LoadAsync<Event>(updatedEvent.EventId);
                    if (existingEvent != null)
                    {
                        existingEvent.IsVisible= updatedEvent.Event_Status;
                        await _dynamoDbContext.SaveAsync(existingEvent);
                        return OkResponse();
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
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Event was Not Updated !!! ",
                StatusCode = 500
            };
        }

            // Autogenerate ID
            public string GenerateId()
        {
            Guid guid = Guid.NewGuid();
            string id = guid.ToString();
            return id;
        }

        // Get Current Epoch
        public static int GetCurrentEpoch()
        {
            DateTimeOffset epochStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            TimeSpan currentTime = DateTimeOffset.UtcNow - epochStart;
            return (int)currentTime.TotalSeconds;
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
