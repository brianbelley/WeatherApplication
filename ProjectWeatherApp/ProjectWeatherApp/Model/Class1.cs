using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjectWeatherApp.Model
{
    public class Main
    {
        public double Temp { get; set; }
        public int Humidity { get; set; }
        public double Pressure { get; set; }
    }

    public class Weather
    {
        public string Main { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
    }

    public class Wind
    {
        public double Speed { get; set; }
    }

    public class Clouds
    {
        public int All { get; set; }
    }

    public class Sys
    {
        public string Country { get; set; }
    }

    public class List
    {
        public int Dt { get; set; }
        public Main Main { get; set; }
        public List<Weather> Weather { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
    }

    public class WeatherData
    {
        public string Name { get; set; }
        public Main Main { get; set; }
        public List<Weather> Weather { get; set; }
        public Wind Wind { get; set; }
        public Clouds Clouds { get; set; }
        public Sys Sys { get; set; }
        public Coord Coord { get; set; }
    }

    public class ForecastData
    {
        public List<List> List { get; set; }
    }

    public class Coord
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }

    public class AutocompleteResult
    {
        public string Name { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
    }

    public class AuthenticationService
    {
        public static string AuthToken { get; set; }
        public static string CurrentUserUid { get; set; }

        private const string FirebaseApiKey = "AIzaSyAYcA1NjGz9zrlbaheOWZqn1SIee5H4bn0";
        private const string SignupUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";

        public async Task<string> Signup(string email, string password)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new
                    {
                        email,
                        password,
                        returnSecureToken = true
                    };

                    var requestBody = JsonConvert.SerializeObject(request);
                    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync($"{SignupUrl}{FirebaseApiKey}", content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        AuthToken = responseData["idToken"];
                        CurrentUserUid = responseData["localId"];
                        return AuthToken;
                    }
                    else
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        throw new Exception("Failed to sign up: " + errorResponse["error"]["message"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to sign up: " + ex.Message);
            }
        }

        public async Task<string> Login(string email, string password)
        {
            string FirebaseApiKey = "AIzaSyAYcA1NjGz9zrlbaheOWZqn1SIee5H4bn0";
            string SignInUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new
                    {
                        email,
                        password,
                        returnSecureToken = true
                    };

                    var requestBody = JsonConvert.SerializeObject(request);
                    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync($"{SignInUrl}{FirebaseApiKey}", content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        AuthToken = responseData["idToken"];
                        CurrentUserUid = responseData["localId"];
                        return AuthToken;
                    }
                    else
                    {
                        var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        throw new Exception(errorResponse["error"]["message"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to log in: " + ex.Message);
            }
        }


    }
}
