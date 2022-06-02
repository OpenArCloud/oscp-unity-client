using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Net;

public class AuthenticationManager : MonoBehaviour
{
   //// the AWS region of where your services live
   //public static Amazon.RegionEndpoint Region = Amazon.RegionEndpoint.EUNorth1;

   //// In production, should probably keep these in a config file
   //const string IdentityPool = "eu-north-1:89aab975-bb8f-489f-9e91-99d032f59bbb"; //insert your Cognito User Pool ID, found under General Settings
   //const string AppClientID = "7ueo10r6crau18pd26q402t79q"; //insert App client ID, found under App Client Settings
   //const string userPoolId = "eu-north-1_E4hfKBxF5";

   //private AmazonCognitoIdentityProviderClient _provider;
   //private CognitoAWSCredentials _cognitoAWSCredentials;
   //private static string _userid = "";
   //private CognitoUser _user;

   //public async Task<bool> RefreshSession()
   //{
   //   Debug.Log("RefreshSession");

   //   DateTime issued = DateTime.Now;
   //   UserSessionCache userSessionCache = new UserSessionCache();
   //   SaveDataManager.LoadJsonData(userSessionCache);

   //   if (userSessionCache != null && userSessionCache._refreshToken != null && userSessionCache._refreshToken != "")
   //   {
   //   try
   //   {
   //      CognitoUserPool userPool = new CognitoUserPool(userPoolId, AppClientID, _provider);

   //      // apparently the username field can be left blank for a token refresh request
   //      CognitoUser user = new CognitoUser("", AppClientID, userPool, _provider);

   //      // The "Refresh token expiration (days)" (Cognito->UserPool->General Settings->App clients->Show Details) is the
   //      // amount of time since the last login that you can use the refresh token to get new tokens. After that period the refresh
   //      // will fail Using DateTime.Now.AddHours(1) is a workaround for https://github.com/aws/aws-sdk-net-extensions-cognito/issues/24
   //      user.SessionTokens = new CognitoUserSession(
   //         userSessionCache.getIdToken(),
   //         userSessionCache.getAccessToken(),
   //         userSessionCache.getRefreshToken(),
   //         issued,
   //         DateTime.Now.AddDays(30)); // TODO: need to investigate further. 
   //                                    // It was my understanding that this should be set to when your refresh token expires...

   //      // Attempt refresh token call
   //      AuthFlowResponse authFlowResponse = await user.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest
   //      {
   //         AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH
   //      })
   //      .ConfigureAwait(false);

   //      // Debug.Log("User Access Token after refresh: " + token);
   //      Debug.Log("User refresh token successfully updated!");

   //      // update session cache
   //      UserSessionCache userSessionCacheToUpdate = new UserSessionCache(
   //         authFlowResponse.AuthenticationResult.IdToken,
   //         authFlowResponse.AuthenticationResult.AccessToken,
   //         authFlowResponse.AuthenticationResult.RefreshToken,
   //         userSessionCache.getUserId());

   //      SaveDataManager.SaveJsonData(userSessionCacheToUpdate);

   //      // update credentials with the latest access token
   //      _cognitoAWSCredentials = user.GetCognitoAWSCredentials(IdentityPool, Region);

   //      _user = user;

   //      return true;
   //   }
   //   catch (NotAuthorizedException ne)
   //   {
   //      // https://docs.aws.amazon.com/cognito/latest/developerguide/amazon-cognito-user-pools-using-tokens-with-identity-providers.html
   //      // refresh tokens will expire - user must login manually every x days (see user pool -> app clients -> details)
   //      Debug.Log("NotAuthorizedException: " + ne);
   //   }
   //   catch (WebException webEx)
   //   {
   //      // we get a web exception when we cant connect to aws - means we are offline
   //      Debug.Log("WebException: " + webEx);
   //   }
   //   catch (Exception ex)
   //   {
   //      Debug.Log("Exception: " + ex);
   //   }
   //   }
   //   return false;
   //}

   //public async Task<bool> Login(string email, string password)
   //{
   //   // Debug.Log("Login: " + email + ", " + password);

   //   CognitoUserPool userPool = new CognitoUserPool(userPoolId, AppClientID, _provider);
   //   CognitoUser user = new CognitoUser(email, AppClientID, userPool, _provider);

   //   InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
   //   {
   //      Password = password
   //   };

   //   try
   //   {
   //
   //      authFlowResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

   //      _userid = await GetUserIdFromProvider(authFlowResponse.AuthenticationResult.AccessToken);
   //      // Debug.Log("Users unique ID from cognito: " + _userid);

   //      UserSessionCache userSessionCache = new UserSessionCache(
   //         authFlowResponse.AuthenticationResult.IdToken,
   //         authFlowResponse.AuthenticationResult.AccessToken,
   //         authFlowResponse.AuthenticationResult.RefreshToken,
   //         _userid);

   //      SaveDataManager.SaveJsonData(userSessionCache);

   //      // This how you get credentials to use for accessing other services.
   //      // This IdentityPool is your Authorization, so if you tried to access using an
   //      // IdentityPool that didn't have the policy to access your target AWS service, it would fail.
   //      _cognitoAWSCredentials = user.GetCognitoAWSCredentials(IdentityPool, Region);

   //      _user = user;

   //      return true;
   //   }
   //   catch (Exception e)
   //   {
   //      Debug.Log("Login failed, exception: " + e);
   //      return false;
   //   }
   //}

   //public async Task<bool> Signup(string username, string email, string password)
   //{
   //   // Debug.Log("SignUpRequest: " + username + ", " + email + ", " + password);

   //   SignUpRequest signUpRequest = new SignUpRequest()
   //   {
   //      ClientId = AppClientID,
   //      Username = email,
   //      Password = password
   //   };

   //   // must provide all attributes required by the User Pool that you configured
   //   List<AttributeType> attributes = new List<AttributeType>()
   //   {
   //      new AttributeType(){
   //         Name = "email", Value = email
   //      },
   //      new AttributeType(){
   //         Name = "preferred_username", Value = username
   //      }
   //   };
   //   signUpRequest.UserAttributes = attributes;

   //   try
   //   {
   //      SignUpResponse sighupResponse = await _provider.SignUpAsync(signUpRequest);
   //      Debug.Log("Sign up successful");
   //      return true;
   //   }
   //   catch (Exception e)
   //   {
   //      Debug.Log("Sign up failed, exception: " + e);
   //      return false;
   //   }
   //}

   //// Make the user's unique id available for GameLift APIs, linking saved data to user, etc
   //public string GetUsersId()
   //{
   //   // Debug.Log("GetUserId: [" + _userid + "]");
   //   if (_userid == null || _userid == "")
   //   {
   //      // load userid from cached session 
   //      UserSessionCache userSessionCache = new UserSessionCache();
   //      SaveDataManager.LoadJsonData(userSessionCache);
   //      _userid = userSessionCache.getUserId();
   //   }
   //   return _userid;
   //}

   //// we call this once after the user is authenticated, then cache it as part of the session for later retrieval 
   //private async Task<string> GetUserIdFromProvider(string accessToken)
   //{
   //   // Debug.Log("Getting user's id...");
   //   string subId = "";

   //   Task<GetUserResponse> responseTask =
   //      _provider.GetUserAsync(new GetUserRequest
   //      {
   //         AccessToken = accessToken
   //      });

   //   GetUserResponse responseObject = await responseTask;

   //   // set the user id
   //   foreach (var attribute in responseObject.UserAttributes)
   //   {
   //      if (attribute.Name == "sub")
   //      {
   //         subId = attribute.Value;
   //         break;
   //      }
   //   }

   //   return subId;
   //}

   //// Limitation note: so this GlobalSignOutAsync signs out the user from ALL devices, and not just the game.
   //// So if you had other sessions for your website or app, those would also be killed.  
   //// Currently, I don't think there is native support for granular session invalidation without some work arounds.
   //public async void SignOut()
   //{
   //   await _user.GlobalSignOutAsync();

   //   // Important! Make sure to remove the local stored tokens 
   //   UserSessionCache userSessionCache = new UserSessionCache("", "", "", "");
   //   SaveDataManager.SaveJsonData(userSessionCache);

   //   Debug.Log("user logged out.");
   //}

   //// access to the user's authenticated credentials to be used to call other AWS APIs
   //public CognitoAWSCredentials GetCredentials()
   //{
   //   return _cognitoAWSCredentials;
   //}

   //// access to the user's access token to be used wherever needed - may not need this at all.
   //public string GetAccessToken()
   //{
   //   UserSessionCache userSessionCache = new UserSessionCache();
   //   SaveDataManager.LoadJsonData(userSessionCache);
   //   return userSessionCache.getAccessToken();
   //}

   //void Awake()
   //{
   //   Debug.Log("AuthenticationManager: Awake");
   //   _provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), Region);
   //}


}
