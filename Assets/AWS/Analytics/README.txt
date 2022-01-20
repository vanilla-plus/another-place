================
Analytics
================

================
Description
================
Analytics is a static analytics library for sending session and video analytics data to AWS DynamoDB for Swinburne-Place.

===============
Requirements
===============
AWSSDK.DynamoDBv2 Unity package
.Net 4.x

===============
Instructions
===============
1) Add an object to your scene that calls AWS' "UnityInitializer.AttachToGameObject(gameObject);" function prior to calling any of Analytics' functions.

2) Add an object to your scene that calls AWS' "AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;" variable assignment prior to calling any of Analytics' functions. This can be the same object as the one used for step 1.

3) Set the target IP address for the internet connection ping test through the variable pingTargetIP. The default value is 8.8.8.8 (Google's DNS Server).

4) Set the timeout duration for the internet connection ping test through the variable pingTimeoutMilliseconds. The default value is 1000 milliseconds (1 second).

5) Call Analytics' functions at appropritate times. Functions and needed info listed below.
	StartSession(string userID) - Starts the Analytics session. Only one session can exist at a time, and every run of the application should only have one session.
	EndSession() - Ends the Analytics session.
	StartVideo(string videoName, string videoDifficulty, string videoGender) - Starts a video analytic. Only one video analytic can exist at a time.
	EndVideo(bool videoInterupted) - Ends a video analytic.

Refer to the included AnalyticsUseExample script for an example of how to use Analytics.