using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web;
using DTPortal.Web.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DTPortal.Web.Helper
{
    public class OpenID
    {
        public IConfiguration configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IGlobalConfiguration _globalConfiguration;
        private readonly string clientId;
        private readonly string clientSecret;
        public OpenID(IConfiguration _configuration,IHttpClientFactory httpClientFactory,IGlobalConfiguration globalConfiguration)
        {
            _httpClientFactory = httpClientFactory;
            configuration = _configuration;
            _globalConfiguration = globalConfiguration;
            clientId = _globalConfiguration.AdminPortalClientId();
            clientSecret = _globalConfiguration.AdminPortalClientSecret();
        }

        public string GetAuthorizationUrl(string nonce, string state)
        {
            try
            {
                var authorizationURl = configuration.GetValue<string>("DTIDP_Config:authorizeUrl");
                //var clientId = configuration["DTIDP_Config:Client_id"];
                //var clientSecret = configuration["DTIDP_Config:client_secret"];
                //if (configuration.GetValue<bool>("EncryptionEnabled"))
                //{
                //    clientId = PKIMethods.Instance.
                //            PKIDecryptSecureWireData(clientId);
                //    clientSecret = PKIMethods.Instance.
                //            PKIDecryptSecureWireData(clientSecret);
                //};
                var isOpenId = configuration.GetValue<bool>("OpenId_Connect");
                if (isOpenId == true)
                {
                    /*Prepare jwtToken object to generate jwt token which is send form
                     request parameter in query string*/
                    var requestObject = new JwtTokenObj();
                    requestObject.Expiry = 60;
                    requestObject.Audience = configuration["DTIDP_Config:Issuer"];
                    requestObject.Issuer = clientId;
                    requestObject.ResponseType = "code";
                    requestObject.RedirecUri = configuration["DTIDP_Config:redirect_Url"];
                    requestObject.Scope = configuration["DTIDP_Config:scope"];
                    requestObject.State = state;
                    requestObject.Nonce = nonce;
                    bool encryptionEnabled = configuration.GetValue<bool>("EncryptionEnabled");
                    //generate jwt token by passing jwttoken object details
                    var response = JWTTokenManager.GenerateJWTToken(requestObject, encryptionEnabled);
                    if (null == response)
                    {
                        throw new Exception("Fail to generate JWT token for " +
                            "Authorization request.");
                    }

                    //generate idp login url using ClientId,ClientId,Scopes,state,nonce,request
                    //check all values in appsettings.Development.json file
                     authorizationURl = authorizationURl +
                        "?client_id={0}&redirect_uri={1}&response_type=code&scope={2}&state={3}&" +
                        "nonce={4}&request={5}";

                    return String.Format(authorizationURl,
                                         configuration.GetValue<string>("DTIDP_Config:Client_id"),
                                         configuration.GetValue<string>("DTIDP_Config:redirect_Url"),
                                         configuration.GetValue<string>("DTIDP_Config:scope"),
                                         state, nonce, response);
                }
                else
                {
                    authorizationURl = authorizationURl +
                       "?client_id={0}&redirect_uri={1}&response_type=code&scope={2}&state={3}";

                    var scope = configuration.GetValue<string>("DTIDP_Config:scope");

                    return String.Format(authorizationURl,
                                         clientId,
                                         configuration.GetValue<string>("DTIDP_Config:redirect_Url"),
                                         scope.Replace("openid ",""),
                                         state);

                }
            }
            catch (Exception)
            {
                throw ;
            }
        }

        public string GetLogoutUrl(string idToken =null, string state=null)
        {
            try
            {
                var isOpenId = configuration.GetValue<bool>("OpenId_Connect");
                if (isOpenId == true)
                {
                    var LogoutURl = configuration.GetValue<string>("DTIDP_Config:EndSessionEndpoint") +
                    "?id_token_hint={0}&post_logout_redirect_uri={1}&state={2}";

                    //generate idp logout url using id_token,PostLogoutRedirectUri and state value
                    return String.Format(LogoutURl, idToken,
                                 configuration.GetValue<string>("DTIDP_Config:logout_url"),
                                 state);
                }
                else
                {
                    return String.Format(configuration.GetValue<string>("DTIDP_Config:signOutUrl"), configuration.GetValue<string>("DTIDP_Config:logout_url"));
                }
            }
            catch (Exception )
            {
                throw ;
            }
        }

        public async Task<JObject> GetAccessToken(string code)
        {
            try
            {
                //var clientId = configuration["DTIDP_Config:Client_id"];
                //var clientSecret = configuration["DTIDP_Config:client_secret"];
                //if (configuration.GetValue<bool>("EncryptionEnabled"))
                //{
                //    clientId = PKIMethods.Instance.
                //            PKIDecryptSecureWireData(clientId);
                //    clientSecret = PKIMethods.Instance.
                //            PKIDecryptSecureWireData(clientSecret);
                //};
                //get token endpoint url from appsetting.Development.json file
                var TokenUrl = configuration.GetValue<string>("DTIDP_Config:dt_tokenUrl");

                //prepare data object which is send with token endpoint url 
                var data = new Dictionary<string, string>
                {
                   { "code", code },
                   { "client_id", clientId },
                   { "redirect_uri", configuration.GetValue<string>("DTIDP_Config:redirect_Url") },
                   { "grant_type", "authorization_code" }
                };

                

                var isOpenId = configuration.GetValue<bool>("OpenId_Connect");
                if (isOpenId == true)
                {
                    //set client assertion type
                    var ClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

                    /*Prepare jwtToken object to generate jwt token which is send form
                      client_assertion parameter in query string*/
                    var requestObject = new JwtTokenObj();
                    requestObject.Expiry = 60;
                    requestObject.Audience = configuration["DTIDP_Config:dt_tokenUrl"];
                    requestObject.Issuer = clientId;
                    requestObject.Subject = clientId;

                    bool encryptionEnabled = configuration.GetValue<bool>("EncryptionEnabled");

                    var ClientAssertion = JWTTokenManager.GenerateJWTToken(requestObject,encryptionEnabled);
                    if (null == ClientAssertion)
                    {
                        throw new Exception("Fail to generate JWT token for Token request.");
                    }

                    data.Add("client_assertion_type", ClientAssertionType);
                    data.Add("client_assertion", ClientAssertion);

                }

                //convert data object in url encoded form
                var content = new FormUrlEncodedContent(data);

                var authToken = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");

                HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
                client.BaseAddress = new Uri(TokenUrl);

                var authzHeader = "Basic  " + Convert.ToBase64String(authToken);
                client.DefaultRequestHeaders.Add(configuration["AccessTokenHeaderName"],
                    authzHeader);
                client.BaseAddress = new Uri(TokenUrl);

                var response = await client.PostAsync(TokenUrl, content);
                if (response == null)
                {
                    throw new Exception("GetAccessToken responce getting null");
                }
                if (!response.IsSuccessStatusCode)
                {
                    dynamic error = new JObject();
                    error.error = response.StatusCode;
                    error.error_description = response.ReasonPhrase;
                    return error;
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    return JObject.Parse(responseString);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<JObject> GetAccessToken()
        {
            try
            {
                var TokenUrl = configuration.GetValue<string>("DTIDP_Config:dt_tokenUrl");

                //prepare data object which is send with token endpoint url 
                var data = new Dictionary<string, string>
                {
                   { "client_id", clientId },
                   { "grant_type", "client_credentials" }
                };

                //convert data object in url encoded form
                var content = new FormUrlEncodedContent(data);

                var authToken = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");

                HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
                client.BaseAddress = new Uri(TokenUrl);

                var authzHeader = "Basic  " + Convert.ToBase64String(authToken);
                client.DefaultRequestHeaders.Add(configuration["AccessTokenHeaderName"],
                    authzHeader);
                client.BaseAddress = new Uri(TokenUrl);

                var response = await client.PostAsync(TokenUrl, content);
                if (response == null)
                {
                    throw new Exception("GetAccessToken responce getting null");
                }
                if (!response.IsSuccessStatusCode)
                {
                    dynamic error = new JObject();
                    error.error = response.StatusCode;
                    error.error_description = response.ReasonPhrase;
                    return error;
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    return JObject.Parse(responseString);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<JObject> GetUserInfo(string accessToken)
        {
            try
            {
                var UserInfoUrl = configuration.GetValue<string>(
                    "DTIDP_Config:dt_userinfoUrl");

                HttpClient client = _httpClientFactory.CreateClient("ignoreSSL");
                client.BaseAddress = new Uri(UserInfoUrl);
                var authzHeader = "Bearer  " + accessToken;
                client.DefaultRequestHeaders.Add(configuration["AccessTokenHeaderName"],
                    authzHeader);

                var response = await client.GetAsync(UserInfoUrl);
                if (response == null)
                {
                    throw new Exception("get user info responce getting null");
                }
                if (!response.IsSuccessStatusCode)
                {
                    dynamic error = new JObject();
                    error.error = response.StatusCode;
                    error.error_description = response.ReasonPhrase;
                    return error;
                }
                else
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    JObject info = JObject.Parse(responseString);
                    return info;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ClaimsPrincipal ValidateIdentityToken(string idToken)
        {
            try
            {
                //validate id_token
                var user = ValidateJwt(idToken);

                //return id_token claim values
                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ClaimsPrincipal ValidateJwt(string jwt)
        {
            try
            {
                //set options for jwt signature validation
                var parameters = new TokenValidationParameters
                {
                    IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                    {
                        /*get key from idp jwks url to validate id_token signature*/
                        var client = new HttpClient();
                        var response = client.GetAsync(configuration["DTIDP_Config:JwksUri"]).Result;
                        var responseString = response.Content.ReadAsStringAsync().Result;
                        var keys = JsonConvert.DeserializeObject<JsonWebKeySet>(responseString);
                        return keys.Keys;
                    },
                    //set flag true for validate issuer
                    ValidateIssuer = true,
                    //set flag true for validate Audience
                    ValidateAudience = true,
                    //set valid issuer to verify in token issuer
                    ValidIssuer = configuration["DTIDP_Config:Issuer"],
                    //set valid Audience to verify in token Audience
                    ValidAudience = configuration["DTIDP_Config:Client_id"], 
                    NameClaimType = "name",
                };

                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();

                //validate jwt token
                // if token is valid it return claim otherwise throw exception
                var user = handler.ValidateToken(jwt, parameters, out var _);
                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
