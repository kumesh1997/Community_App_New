using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class S3BucketService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly RegionEndpoint BucketRegion = RegionEndpoint.USEast1; // Change this to your bucket's region
        public S3BucketService()
        {
            _s3Client = new AmazonS3Client(BucketRegion);
            _bucketName = "sltappbucket";
        }

        public string UploadImageAndGetUrl(string image64Base, string fileName)
        {
            // Decode the base64 image string to bytes
            byte[] imageBytes = Convert.FromBase64String(image64Base);
            int sizeInBytes = imageBytes.Length;

            double sizeInKilobytes = sizeInBytes / 1024.0;

            if (sizeInKilobytes > 200)
            {
                // Handle the case where the image size is greater than 100 KB
                return null;
            }
            else
            {
                // Handle the case where the image size is within 100 KB
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName,
                    InputStream = new MemoryStream(imageBytes),
                    ContentType = "image/jpeg" // Adjust the content type based on your image format.
                };

                var response = _s3Client.PutObjectAsync(putRequest).GetAwaiter().GetResult();

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // Generate the link to access the uploaded image.
                    return _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
                    {
                        BucketName = _bucketName,
                        Key = fileName,
                        Expires = DateTime.UtcNow.AddYears(5)
                        // The link will expire in 24 hours, you can adjust the expiration time as needed.
                    });
                }
            }
            // Handle the case when image upload fails.
            return null;
        }
    }
}
