// Copyright 2020 Okta Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Threading.Tasks;
#if UNITY_EDITOR
using System.Net.Sockets;
using System.Runtime.InteropServices;
#endif

public delegate string HandleLoginResponse(string urlResponse);

public class OAuth2Authentication : MonoBehaviour
{
   
    // client configuration
    [SerializeField] string clientID;
    [SerializeField] string clientSecret;
    [SerializeField] string authorizationEndpoint;
    [SerializeField] string tokenEndpoint;
    [SerializeField] string userInfoEndpoint;
    [SerializeField] string redirectURI;

    private string _code_verifier;
    private string _sentState;

    public static event Action<bool> IsAuthenticated;


    private void OnEnable()
    {
        DeepLinkManager.HandleDeepLink += HandleDeepLinkResponse;
    }

    private void OnDisable()
    {
        DeepLinkManager.HandleDeepLink -= HandleDeepLinkResponse;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void DoAuthenticate()
    {
#if UNITY_EDITOR
        //CheckLocalTokenChache();

        AuthenticationInEditor();
#else
        CheckLocalTokenChache();
        //Add logic to check current auth state and only doOAuth() when needed.
       // Authenticate();
#endif

    }

    //TODO: Using this method to validate token until refresh_token functionality is implemented
    public async void CheckLocalTokenChache()
    {
        // add check if file dosent exist
        
        UserSessionCache userSessionCache = new UserSessionCache();
        SaveDataManager.LoadJsonData(userSessionCache);
     
        if(!string.IsNullOrEmpty(userSessionCache.getAccessToken()))
        {
          
           GetUserInformtion(userSessionCache.getAccessToken());
           bool isTokenValid = await CheckTokenValidity(userSessionCache.getAccessToken());

            if(isTokenValid)
            {
                IsAuthenticated?.Invoke(true);
                return;
            }
            else
            {
                Authenticate();
            }
        }
        else
        {
            Authenticate();
        }

        IsAuthenticated?.Invoke(false);
#if UNITY_EDITOR
        AuthenticationInEditor();
#endif
    }

    public void Authenticate()
    {

        _sentState = string.Empty;
        _code_verifier = string.Empty;

        // Generates state and PKCE values.
        _sentState = randomDataBase64url(32);
        _code_verifier = randomDataBase64url(32);
        string code_challenge = base64urlencodeNoPadding(sha256(_code_verifier));
        const string code_challenge_method = "S256";

        // Creates the OAuth 2.0 authorization request.
        string authorizationRequest = string.Format(
            "{0}?" +
            "response_type=code" +
            "&scope=openid%20profile%20read:scrs%20delete:scrs%20create:scrs%20update:scrs%20offline_access" +
            "&redirect_uri={1}" +
            "&client_id={2}" +
            "&state={3}" +
            "&code_challenge={4}" +
            "&code_challenge_method={5}" +
            "&audience=https://scd.orbit-lab.org",
            authorizationEndpoint,
            System.Uri.EscapeDataString(redirectURI),
            clientID,
            _sentState,
            code_challenge,
            code_challenge_method);

        //Opens request in the browser.
        Application.OpenURL(authorizationRequest);

    }

    public void HandleDeepLinkResponse(string urlResponse)
    {
        //Debug.Log("From HandleDeepLinkResponse method urlResponse: " + urlResponse);

        string codeStartString = "?code=";
        string stateStartString = "&state=";

        int codeStartIndex = urlResponse.IndexOf(codeStartString) + codeStartString.Length;
        int stateStartIndex = urlResponse.IndexOf(stateStartString) + stateStartString.Length;

        string accessCode = urlResponse.Substring(codeStartIndex, (stateStartIndex - stateStartString.Length) - codeStartIndex);
        string incomingState = urlResponse.Substring(stateStartIndex);


        // Compares the receieved state to the expected value, to ensure that
        // this app made the request which resulted in authorization.
        if (!string.Equals(incomingState, _sentState))
        {
            output(String.Format("Received request with invalid state ({0})", incomingState));
            return;
        }

        //Starts the code exchange at the Token Endpoint.
        performCodeExchange(accessCode, _code_verifier, redirectURI);

    }

    async void performCodeExchange(string code, string code_verifier, string redirectURI)
    {
        output("Exchanging code for tokens...");

        // builds the  request
        string tokenRequestBody = string.Format(
            "&code={0}" +
            "&redirect_uri={1}" +
            "&client_id={2}" +
            "&code_verifier={3}" +
            "&client_secret={4}" +
            "&grant_type=authorization_code",
            code,
            System.Uri.EscapeDataString(redirectURI),
            clientID,
            code_verifier,
            clientSecret);

        // sends the request
        HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
        tokenRequest.Method = "POST";
        tokenRequest.ContentType = "application/x-www-form-urlencoded";
        //tokenRequest.Accept = "Accept=application/json;charset=UTF-8";
        byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
        tokenRequest.ContentLength = _byteVersion.Length;
        Stream stream = tokenRequest.GetRequestStream();
        await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
        stream.Close();

        try
        {
            // gets the response
            WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
            using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
            {
                // reads response body
                string responseText = await reader.ReadToEndAsync();

                // converts to dictionary
                Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                //Debug.Log("Code exhange tokens result: " + responseText);

                /*
                foreach (var item in tokenEndpointDecoded)
                {
                    output(String.Format("Items in TokenEndPointDecoded: " + "Key: {0} Value: {1}", item.Key, item.Value));
                }*/
                            
                UserSessionCache userSessionCache = new UserSessionCache(
                  tokenEndpointDecoded["id_token"],
                   tokenEndpointDecoded["access_token"],
                   String.Empty  //Refresh token not implemented
                   );

                SaveDataManager.SaveJsonData(userSessionCache);

                //Get user information
                GetUserInformtion(tokenEndpointDecoded["access_token"]);

                IsAuthenticated?.Invoke(true);
            }
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    output("HTTP: " + response.StatusCode);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        // reads response body
                        string responseText = await reader.ReadToEndAsync();
                        output(responseText);
                    }
                }
            }
        }
    }

    async void GetUserInformtion(string access_token)
    {
        output("Making API Call to Userinfo...");

        // sends the request
        HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userInfoEndpoint);
        userinfoRequest.Method = "GET";
        userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
        userinfoRequest.ContentType = "application/x-www-form-urlencoded";
        //userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        // gets the response
        WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
        using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
        {
            // reads response body
            string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
          //  output("User information response: " +userinfoResponseText);
        }
    }

    // Using this method to validate token until refresh_token functionality is implemented
    async Task<bool> CheckTokenValidity(string access_token)
    {
        output("Making API Call to Userinfo...");

        // sends the request
        HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userInfoEndpoint);
        userinfoRequest.Method = "GET";
        userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
        userinfoRequest.ContentType = "application/x-www-form-urlencoded";

        try
        {
            // gets the response
            WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
            using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
            {
                // reads response body
                string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
                output("User information response: " + userinfoResponseText);
                return true;
            }
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    output("HTTP ERROR: " + response.StatusCode);
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        // reads response body
                        string responseText = await reader.ReadToEndAsync();
                        output(responseText);
                        
                    }
                }
            }
            return false;
        }    
    }


    /// <summary>
    /// Appends the given string to the on-screen log, and the debug console.
    /// </summary>
    /// <param name="output">string to be appended</param>
    public void output(string output)
    {
        //Console.WriteLine(output);
        Debug.Log(output);
    }

    /// <summary>
    /// Returns URI-safe data with a given input length.
    /// </summary>
    /// <param name="length">Input length (nb. output will be longer)</param>
    /// <returns></returns>
    public static string randomDataBase64url(uint length)
    {
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        byte[] bytes = new byte[length];
        rng.GetBytes(bytes);
        return base64urlencodeNoPadding(bytes);
    }

    /// <summary>
    /// Returns the SHA256 hash of the input string.
    /// </summary>
    /// <param name="inputStirng"></param>
    /// <returns></returns>
    public static byte[] sha256(string inputStirng)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
        SHA256Managed sha256 = new SHA256Managed();
        return sha256.ComputeHash(bytes);
    }

    /// <summary>
    /// Base64url no-padding encodes the given input buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static string base64urlencodeNoPadding(byte[] buffer)
    {
        string base64 = Convert.ToBase64String(buffer);

        // Converts base64 to base64url.
        base64 = base64.Replace("+", "-");
        base64 = base64.Replace("/", "_");
        // Strips padding.
        base64 = base64.Replace("=", "");

        return base64;
    }

#if UNITY_EDITOR
    #region Authentication when running in Editor

    // ref http://stackoverflow.com/a/3978040
    public static int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private async void AuthenticationInEditor()
    {

        _sentState = string.Empty;
        _code_verifier = string.Empty;

        // Generates state and PKCE values.
        _sentState = randomDataBase64url(32);
        _code_verifier = randomDataBase64url(32);
        string code_challenge = base64urlencodeNoPadding(sha256(_code_verifier));
        const string code_challenge_method = "S256";


        // Creates a redirect URI using an available port on the loopback address.
        string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, /*GetRandomUnusedPort()*/51772);
        output("redirect URI: " + redirectURI);

        // Creates an HttpListener to listen for requests on that redirect URI.
        var http = new HttpListener();
        http.Prefixes.Add(redirectURI);
        output("Listening..");
        http.Start();

        // Creates the OAuth 2.0 authorization request.
        string authorizationRequest = string.Format(
            "{0}?" +
            "response_type=code" +
            "&scope=openid%20profile%20read:scrs%20delete:scrs%20create:scrs%20update:scrs%20offline_access" +
            "&redirect_uri={1}" +
            "&client_id={2}" +
            "&state={3}" +
            "&code_challenge={4}" +
            "&code_challenge_method={5}" +
            "&audience=https://scd.orbit-lab.org",
            authorizationEndpoint,
            System.Uri.EscapeDataString(redirectURI),
            clientID,
            _sentState,
            code_challenge,
            code_challenge_method);



        // Opens request in the browser.
        System.Diagnostics.Process.Start(authorizationRequest);

        // Waits for the OAuth authorization response.
        var context = await http.GetContextAsync();

        //Brings the Console to Focus.
        BringConsoleToFront();

        // Sends an HTTP response to the browser.
        var response = context.Response;
        string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://scd.orbit-lab.org'></head><body>Please return to the app.</body></html>");
        var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        var responseOutput = response.OutputStream;
        Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
        {
            responseOutput.Close();
            http.Stop();
            Console.WriteLine("HTTP server stopped.");
        });

        // Checks for errors.
        if (context.Request.QueryString.Get("error") != null)
        {
            output(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
            return;
        }
        if (context.Request.QueryString.Get("code") == null
            || context.Request.QueryString.Get("state") == null)
        {
            output("Malformed authorization response. " + context.Request.QueryString);
            return;
        }

        // extracts the code
        var code = context.Request.QueryString.Get("code");
        var incoming_state = context.Request.QueryString.Get("state");

        // Compares the receieved state to the expected value, to ensure that
        // this app made the request which resulted in authorization.
        if (incoming_state != _sentState)
        {
            output(String.Format("Received request with invalid state ({0})", incoming_state));
            return;
        }
        output("Authorization code: " + code);

        // Starts the code exchange at the Token Endpoint.
        performCodeExchange(code, _code_verifier, redirectURI);
    }

    // Hack to bring the Console window to front.
    // ref: http://stackoverflow.com/a/12066376

    [DllImport("kernel32.dll", ExactSpelling = true)]
    public static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    public void BringConsoleToFront()
    {
        SetForegroundWindow(GetConsoleWindow());
    }

    #endregion
#endif

  
}

