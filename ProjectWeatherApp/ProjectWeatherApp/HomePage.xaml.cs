using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using ProjectWeatherApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProjectWeatherApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        private static readonly string api = "a2efb35666c4f4327b66b2a93870e256";
        private static readonly string url = "https://api.openweathermap.org/data/2.5/weather";
        private static readonly string forecastUrl = "https://api.openweathermap.org/data/2.5/forecast?lat={0}&lon={1}&appid={2}";
        private static readonly string autocompleteUrl = "https://api.example.com/autocomplete?q={0}";

        private List<string> searchHistory = new List<string>();

        private FirebaseClient firebase;
        private WeatherData currentWeatherData;

        public HomePage()
        {
            InitializeComponent();
            firebase = new FirebaseClient("https://xamarin-weatherapp-default-rtdb.firebaseio.com/");
            FetchWeatherData("MONTREAL"); // Fetch weather data for the default location
        }

        private async void OnSearchButtonPressed(object sender, EventArgs e)
        {
            var cityName = searchBar.Text;
            if (!string.IsNullOrWhiteSpace(cityName))
            {

                // Add the searched city to the history list
                AddToHistory(cityName);

                // Navigate to the HistoryPage passing the search history list
                await Navigation.PushAsync(new HistoryPage(searchHistory));


                FetchWeatherData(cityName);
            }
        }

        private void AddToHistory(string cityName)
        {
            // Add the searched city to the search history list
            searchHistory.Add(cityName);
        }

        private async void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBar = (SearchBar)sender;
            var searchText = e.NewTextValue;

            // Call your autocomplete API to fetch suggestions based on the searchText
            var suggestions = await GetAutocompleteSuggestions(searchText);

            // Update the ListView's item source with the suggestions
            suggestionListView.ItemsSource = suggestions;

            // Show or hide the suggestion list based on whether there are suggestions
            suggestionListView.IsVisible = suggestions.Any();
        }

        private void OnSuggestionSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var selectedSuggestion = (string)e.SelectedItem;

            // Perform any action with the selected suggestion, such as updating the search bar text
            searchBar.Text = selectedSuggestion;

            // Hide the suggestion list
            suggestionListView.IsVisible = false;
        }

        public async Task<List<string>> GetAutocompleteSuggestions(string searchText)
        {
            try
            {
                // Construct the URL with the search text
                var url = string.Format(autocompleteUrl, searchText);

                // Create an instance of HttpClient
                using (var client = new HttpClient())
                {
                    // Send a GET request to the API
                    var response = await client.GetAsync(url);

                    // If the request is successful, parse the response
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        // Deserialize the JSON response into a list of strings (suggestions)
                        var suggestions = JsonConvert.DeserializeObject<List<string>>(content);

                        return suggestions;
                    }
                    else
                    {
                        // If the request fails, return an empty list
                        return new List<string>();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network errors)
                Console.WriteLine($"Error fetching autocomplete suggestions: {ex.Message}");
                return new List<string>();
            }
        }

        private async void FetchWeatherData(string cityName)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync($"{url}?q={cityName}&appid={api}");
                    var weatherData = JsonConvert.DeserializeObject<WeatherData>(response);

                    // Update UI with current weather data
                    lblCity.Text = $"{weatherData.Name}, {weatherData.Sys.Country}";

                    // Convert temperature from Kelvin to Celsius
                    double temperatureKelvin = weatherData.Main.Temp;
                    double temperatureCelsius = Math.Round(temperatureKelvin - 273.15);
                    lblTemperature.Text = $"{temperatureCelsius}°C";
                    lblHumidity.Text = $"{weatherData.Main.Humidity} %";
                    lblDescription.Text = $"{weatherData.Weather[0].Main}, {weatherData.Weather[0].Description}";
                    lblWindSpeed.Text = $"{weatherData.Wind.Speed} kmph";
                    lblCloudiness.Text = $"{weatherData.Clouds.All} %";
                    lblPressure.Text = $"{weatherData.Main.Pressure} mb";

                    // Load weather icon using URL
                    string iconUrl = $"https://openweathermap.org/img/wn/{weatherData.Weather[0].Icon}.png";
                    imgWeather.Source = ImageSource.FromUri(new Uri(iconUrl));

                    // Store the weather data
                    currentWeatherData = weatherData;

                    // Fetch and display 5-day forecast
                    await FetchAndDisplayForecast(weatherData.Coord.Lat, weatherData.Coord.Lon);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }


        private async Task FetchAndDisplayForecast(double latitude, double longitude)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(string.Format(forecastUrl, latitude, longitude, api));
                    var forecastData = JsonConvert.DeserializeObject<ForecastData>(response);

                    // Clear existing forecast views
                    stkForecast.Children.Clear();

                    // Group forecast data by unique dates (day of the week)
                    var groupedForecasts = forecastData.List.GroupBy(f => DateTimeOffset.FromUnixTimeSeconds(f.Dt).DateTime.Date);

                    // Display 5-day forecast (one forecast per day)
                    foreach (var group in groupedForecasts)
                    {
                        var forecastView = new StackLayout();

                        // Use the first forecast in the group for the day
                        var firstForecast = group.First();

                        var lblDay = new Label { Text = DateTimeOffset.FromUnixTimeSeconds(firstForecast.Dt).DateTime.ToString("ddd"), FontSize = 18 };

                        double temperatureKelvin = firstForecast.Main.Temp;
                        double temperatureCelcius = Math.Round(temperatureKelvin - 273.15);

                        var lblTemp = new Label { Text = $"{temperatureCelcius}°C", FontSize = 18 };

                        var imgIcon = new Image { Source = ImageSource.FromUri(new Uri($"https://openweathermap.org/img/wn/{firstForecast.Weather[0].Icon}.png")), HeightRequest = 50, WidthRequest = 50 };

                        forecastView.Children.Add(lblDay);
                        forecastView.Children.Add(lblTemp);
                        forecastView.Children.Add(imgIcon);

                        stkForecast.Children.Add(forecastView);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Get the current user UID from the AuthenticationService
                var currentUserUid = AuthenticationService.CurrentUserUid;

                // If the current user UID is available
                if (!string.IsNullOrEmpty(currentUserUid))
                {
                    // Save the weather data under the specific user UID
                    if (currentWeatherData != null)
                    {
                        var firebase = new FirebaseClient("https://xamarin-weatherapp-default-rtdb.firebaseio.com");

                        // Serialize the weather data object to JSON
                        var jsonData = JsonConvert.SerializeObject(currentWeatherData);

                        // Push the serialized data to the Realtime Database under the specific user's UID
                        var response = await firebase
                            .Child("users")
                            .Child(currentUserUid)
                            .Child("favorites")
                            .PostAsync(jsonData);

                        // Handle the response if needed
                        Console.WriteLine($"Data pushed successfully. Key: {response.Key}");

                        await DisplayAlert("Success", "Weather data saved to Firebase.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No weather data available.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "User UID not available.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine($"Error saving data: {ex.Message}");
                await DisplayAlert("Error", "Failed to save weather data.", "OK");
            }
        }


        private async Task PushDataToFirebase(WeatherData data)
        {
            try
            {
                // Serialize the data object to JSON
                var jsonData = JsonConvert.SerializeObject(data);

                // Push the serialized data to the Realtime Database
                await firebase.Child("weatherData").PostAsync(jsonData);

                Console.WriteLine($"Data pushed successfully to Firebase.");
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine($"Error pushing data to Firebase: {ex.Message}");
            }
        }
    }
}   