using Firebase.Database;
using Firebase.Database.Query;
using ProjectWeatherApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProjectWeatherApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FavoritesPage : ContentPage
    {
        private readonly FirebaseClient firebaseClient;

        public FavoritesPage()
        {
            InitializeComponent();

            // Initialize FirebaseClient with your Firebase Realtime Database URL
            firebaseClient = new FirebaseClient("https://xamarin-weatherapp-default-rtdb.firebaseio.com");

            // Load the list of cities from the database when the page is created
            LoadCities();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Load the list of cities from the database when the page appears
            LoadCities();
        }

        private async void LoadCities()
        {
            try
            {
                // Get the current user's UID from the AuthenticationService
                string currentUserUid = AuthenticationService.CurrentUserUid;

                // If the user is logged in
                if (!string.IsNullOrEmpty(currentUserUid))
                {
                    // Create a FirebaseClient instance with your Firebase Realtime Database URL
                    var firebaseClient = new FirebaseClient("https://xamarin-weatherapp-default-rtdb.firebaseio.com");

                    // Create a FirebaseObject reference to the "favorites" node under the current user's UID
                    var favoritesNode = firebaseClient.Child("users").Child(currentUserUid).Child("favorites");

                    // Retrieve the list of cities saved under the "favorites" node
                    var citiesSnapshot = await favoritesNode.OnceSingleAsync<Dictionary<string, Dictionary<string, object>>>();

                    // Extract city names from the list of cities
                    var cityNames = citiesSnapshot.Values.Select(city => (string)city["Name"]).ToList();

                    // Bind the list of city names to the ListView
                    citiesListView.ItemsSource = cityNames;
                }
                else
                {
                    await DisplayAlert("Error", "User not logged in.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load cities: {ex.Message}", "OK");
            }
        }



    }
}