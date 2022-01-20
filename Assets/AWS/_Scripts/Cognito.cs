using Amazon;
using Amazon.CognitoIdentity;

namespace AWS
{
    public static class Cognito
    {
        private const string identityPoolId = "ap-southeast-2:b5d45675-818f-4635-86c6-c8f3915280c2";
        private static readonly RegionEndpoint regionEndpoint = RegionEndpoint.APSoutheast2; //AWSConfigs.RegionEndpoint;
        public static readonly CognitoAWSCredentials credentials = new CognitoAWSCredentials(identityPoolId, regionEndpoint);
    }
}
