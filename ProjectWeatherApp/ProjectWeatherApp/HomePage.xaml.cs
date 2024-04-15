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
            FetchWeatherData("MONTREAL"); 
        }

        public HomePage(string defaultCity)
        {
            InitializeComponent();
            firebase = new FirebaseClient("https://xamarin-weatherapp-default-rtdb.firebaseio.com/");
            FetchWeatherData(defaultCity);
        }

        private async void OnSearchButtonPressed(object sender, EventArgs e)
        {
            var cityName = searchBar.Text;
            if (!string.IsNullOrWhiteSpace(cityName))
            {

            
                AddToHistory(cityName);

             
                await Navigation.PushAsync(new HistoryPage(searchHistory));


                FetchWeatherData(cityName);
            }
        }

        private void AddToHistory(string cityName)
        {
          
            searchHistory.Add(cityName);
        }

        private async void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchBar = (SearchBar)sender;
            var searchText = e.NewTextValue;

           
            var suggestions = await GetAutocompleteSuggestions(searchText);

         
            suggestionListView.ItemsSource = suggestions;

         
            suggestionListView.IsVisible = suggestions.Any();
        }

        //Still in maintenance
        private void OnSuggestionSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var selectedSuggestion = (string)e.SelectedItem;

         
            searchBar.Text = selectedSuggestion;

           
            suggestionListView.IsVisible = false;
        }

        //Still in maintenance
        public async Task<List<string>> GetAutocompleteSuggestions(string searchText)
        {
            try
            {
               
                var url = string.Format(autocompleteUrl, searchText);

             
                using (var client = new HttpClient())
                {
                  
                    var response = await client.GetAsync(url);

                  
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                       
                        var suggestions = JsonConvert.DeserializeObject<List<string>>(content);

                        return suggestions;
                    }
                    else
                    {
                       
                        return new List<string>();
                    }
                }
            }
            catch (Exception ex)
            {
              
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

                  
                    lblCity.Text = $"{weatherData.Name}, {weatherData.Sys.Country}";

                   
                    double temperatureKelvin = weatherData.Main.Temp;
                    double temperatureCelsius = Math.Round(temperatureKelvin - 273.15);
                    lblTemperature.Text = $"{temperatureCelsius}°C";
                    lblHumidity.Text = $"{weatherData.Main.Humidity} %";
                    lblDescription.Text = $"{weatherData.Weather[0].Main}, {weatherData.Weather[0].Description}";
                    lblWindSpeed.Text = $"{weatherData.Wind.Speed} kmph";
                    lblCloudiness.Text = $"{weatherData.Clouds.All} %";
                    lblPressure.Text = $"{weatherData.Main.Pressure} mb";

                   
                    string iconUrl = $"https://openweathermap.org/img/wn/{weatherData.Weather[0].Icon}.png";
                    imgWeather.Source = ImageSource.FromUri(new Uri(iconUrl));

                   
                    currentWeatherData = weatherData;

                   
                    await FetchAndDisplayForecast(weatherData.Coord.Lat, weatherData.Coord.Lon);
                }
            }
            catch (Exception ex)
            {
               
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

                    // Clear the existing forecast stack layout
                    stkForecast.Children.Clear();

                    // Group the forecasts by date
                    var groupedForecasts = forecastData.List
                        .Where(f => DateTimeOffset.FromUnixTimeSeconds(f.Dt).Date > DateTime.Now.Date) // Exclude the current day
                        .GroupBy(f => DateTimeOffset.FromUnixTimeSeconds(f.Dt).DateTime.Date);

                    foreach (var group in groupedForecasts)
                    {
                        var forecastView = new StackLayout();

                     
                        var lblDay = new Label { Text = DateTimeOffset.FromUnixTimeSeconds(group.First().Dt).DateTime.ToString("ddd"), FontSize = 18 };

                     
                        double temperatureKelvin = group.First().Main.Temp;
                        double temperatureCelcius = Math.Round(temperatureKelvin - 273.15);
                        var lblTemp = new Label { Text = $"{temperatureCelcius}°C", FontSize = 18 };

         
                        var imgIcon = new Image { Source = ImageSource.FromUri(new Uri($"https://openweathermap.org/img/wn/{group.First().Weather[0].Icon}.png")), HeightRequest = 50, WidthRequest = 50 };

             
                        forecastView.Children.Add(lblDay);
                        forecastView.Children.Add(lblTemp);
                        forecastView.Children.Add(imgIcon);

                        stkForecast.Children.Add(forecastView);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }


        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            try
            {

                var currentUserUid = AuthenticationService.CurrentUserUid;

                if (!string.IsNullOrEmpty(currentUserUid))
                {

                    if (currentWeatherData != null)
                    {
                        var firebase = new FirebaseClient("https://xamarin-weatherapp-default-rtdb.firebaseio.com");


                        var jsonData = JsonConvert.SerializeObject(currentWeatherData);


                        var response = await firebase
                            .Child("users")
                            .Child(currentUserUid)
                            .Child("favorites")
                            .PostAsync(jsonData);

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

                Console.WriteLine($"Error saving data: {ex.Message}");
                await DisplayAlert("Error", "Failed to save weather data.", "OK");
            }
        }


        private async Task PushDataToFirebase(WeatherData data)
        {
            try
            {

                var jsonData = JsonConvert.SerializeObject(data);


                await firebase.Child("weatherData").PostAsync(jsonData);

                Console.WriteLine($"Data pushed successfully to Firebase.");
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error pushing data to Firebase: {ex.Message}");
            }
        }

        public void LoadCityInSearch(string cityName)
        {
            searchBar.Text = cityName;
        }

    }
}   