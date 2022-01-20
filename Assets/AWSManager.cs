using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon;
using Amazon.CognitoIdentity;
public static class AWSManager
{
    // Settings
    private static readonly RegionEndpoint
        cognitoRegionOverride = null,
        dynamoDBRegionOverride = null,
        s3RegionOverride = null;

    private const string cognitoIdentityPoolId = "";

    // Variables
    private static CognitoAWSCredentials _cognitoCredentials;

    // Getters
    private static RegionEndpoint cognitoRegion { get { return cognitoRegionOverride == null ? AWSConfigs.RegionEndpoint : cognitoRegionOverride; } }
    private static RegionEndpoint dynamoDBRegion { get { return dynamoDBRegionOverride == null ? AWSConfigs.RegionEndpoint : dynamoDBRegionOverride; } }
    private static RegionEndpoint s3Region { get { return s3RegionOverride == null ? AWSConfigs.RegionEndpoint : s3RegionOverride; } }

    private static CognitoAWSCredentials cognitoCredentials { get { if (_cognitoCredentials == null) _cognitoCredentials = new CognitoAWSCredentials(cognitoIdentityPoolId, cognitoRegion); return _cognitoCredentials; } }
}
