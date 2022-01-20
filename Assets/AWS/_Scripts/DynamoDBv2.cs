using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace AWS
{
    public static class DynamoDBv2
    {
        private static readonly RegionEndpoint regionEndpoint = RegionEndpoint.APSoutheast2;
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient(Cognito.credentials, regionEndpoint);

        public static async Task<PutItemResponse> PutItemAsync(string tableName, Dictionary<string, AttributeValue> entry)
        {
            PutItemRequest request = new PutItemRequest
            {
                TableName = tableName,
                Item = entry
            };

            return await client.PutItemAsync(request);
        }
    }
}
