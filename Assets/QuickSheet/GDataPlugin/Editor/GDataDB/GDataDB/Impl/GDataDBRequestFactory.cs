///////////////////////////////////////////////////////////////////////////////
///
/// GDataDBRequestFactory.cs
/// 
/// (c)2015 Kim, Hyoun Woo
///
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityQuickSheet;

namespace GDataDB.Impl
{
    /// <summary>
    /// Handles OAuth2 credentials to access on google spreadsheets. 
    /// 
    /// Note that it needs json type of private key to get access code.
    /// 
    /// </summary>
    public class GDataDBRequestFactory
    {
        const string SCOPE = "https://www.googleapis.com/auth/drive https://spreadsheets.google.com/feeds https://docs.google.com/feeds";
        const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";
        const string TOKEN_TYPE = "refresh";

        public static GOAuth2RequestFactory RefreshAuthenticate()
        {
            if (string.IsNullOrEmpty(GoogleDataSettings.Instance._RefreshToken) ||
                string.IsNullOrEmpty(GoogleDataSettings.Instance._AccessToken))
                return null;

            if (string.IsNullOrEmpty(GoogleDataSettings.Instance.OAuth2Data.client_id) ||
                string.IsNullOrEmpty(GoogleDataSettings.Instance.OAuth2Data.client_id))
                return null;

            OAuth2Parameters parameters = new OAuth2Parameters()
            {
                RefreshToken = GoogleDataSettings.Instance._RefreshToken,
                AccessToken = GoogleDataSettings.Instance._AccessToken,
                ClientId = GoogleDataSettings.Instance.OAuth2Data.client_id,
                ClientSecret = GoogleDataSettings.Instance.OAuth2Data.client_secret,
                Scope = "https://www.googleapis.com/auth/drive https://spreadsheets.google.com/feeds",
                AccessType = "offline",
                TokenType = "refresh"
            };
            return new GOAuth2RequestFactory("spreadsheet", "MySpreadsheetIntegration-v1", parameters);
        }

        public static void InitAuthenticate()
        {
            string clientId = GoogleDataSettings.Instance.OAuth2Data.client_id;
            string clientSecret = GoogleDataSettings.Instance.OAuth2Data.client_secret;
            string accessCode = GoogleDataSettings.Instance._AccessCode;

            // OAuth2Parameters holds all the parameters related to OAuth 2.0.
            OAuth2Parameters parameters = new OAuth2Parameters();
            parameters.ClientId = clientId;
            parameters.ClientSecret = clientSecret;
            parameters.RedirectUri = REDIRECT_URI;

            // Retrieves the Authorization URL
            parameters.Scope = SCOPE;
            parameters.AccessType = "offline"; // IMPORTANT 
            parameters.TokenType = TOKEN_TYPE; // IMPORTANT 

            string authorizationUrl = OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
            Debug.Log(authorizationUrl);
            //Debug.Log("Please visit the URL above to authorize your OAuth "
            //      + "request token.  Once that is complete, type in your access code to "
            //      + "continue...");

            parameters.AccessCode = accessCode;

            if (IsValidURL(authorizationUrl))
            {
                Application.OpenURL(authorizationUrl);
                const string message = @"Copy the 'Access Code' on your browser into the access code textfield.";
                EditorUtility.DisplayDialog("Info", message, "OK");
            }
            else
                EditorUtility.DisplayDialog("Error", "Invalid URL: " + authorizationUrl, "OK");
        }

        /// <summary>
        ///  Check whether the given string is a valid http or https URL.
        /// </summary>
        private static bool IsValidURL(string url)
        {
            Uri uriResult;
            return (Uri.TryCreate(url, UriKind.Absolute, out uriResult) && 
                                (uriResult.Scheme == Uri.UriSchemeHttp || 
                                 uriResult.Scheme == Uri.UriSchemeHttps));
        }

        public static void FinishAuthenticate()
        {
            OAuth2Parameters parameters = new OAuth2Parameters();
            parameters.ClientId = GoogleDataSettings.Instance.OAuth2Data.client_id;
            parameters.ClientSecret = GoogleDataSettings.Instance.OAuth2Data.client_secret;
            parameters.RedirectUri = REDIRECT_URI;

            parameters.Scope = SCOPE;
            parameters.AccessType = "offline"; // IMPORTANT 
            parameters.TokenType = TOKEN_TYPE; // IMPORTANT 

            parameters.AccessCode = GoogleDataSettings.Instance._AccessCode;

            OAuthUtil.GetAccessToken(parameters);
            string accessToken = parameters.AccessToken;
            string refreshToken = parameters.RefreshToken;
            //Debug.Log("OAuth Access Token: " + accessToken + "\n");
            //Debug.Log("OAuth Refresh Token: " + refreshToken + "\n");

            GoogleDataSettings.Instance._RefreshToken = refreshToken;
            GoogleDataSettings.Instance._AccessToken = accessToken;
        }
    }
}