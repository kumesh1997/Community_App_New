using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Model;
using AWSLambdacommunityapp.Service;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdacommunityapp;

public class Function
{
    // Reference for Help Desk Service
    private readonly HelpDeskService helpDeskService;
    // Reference for Visitor Service
    private readonly VisitorService visitorService;
    // Reference for Amenities Service
    private readonly AmenitiesService amenitiesService;
    // Reference for User Management Service
    private readonly UserManagementService userManagementService;

    public Function()
    {
        // New Help Desk Service Instance
        helpDeskService = new HelpDeskService();

        // New Visitor Service Instance
        visitorService = new VisitorService();

        // New Amenities Service Instance
        amenitiesService = new AmenitiesService();

        // New User Management Service Instance
        userManagementService = new UserManagementService();
    }

   public async Task<APIGatewayHttpApiV2ProxyResponse> HelpDeskHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await helpDeskService.HelpDeskFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> VisitorHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await visitorService.VisitorFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> AmenitiesHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await amenitiesService.AmenitiesFunctionHandler(request, context);
    }


    public async Task<APIGatewayHttpApiV2ProxyResponse> UserHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await userManagementService.VisitorFunctionHandler(request, context);
    }

}
