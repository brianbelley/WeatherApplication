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

           
            firebaseClient = new FirebaseClient("https://xamarin-weatherapp-default-rtdb.firebaseio.com");

            LoadCities();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

         
            LoadCities();
        }

        private async void LoadCities()
        {
            try
            {
             
                string currentUserUid = AuthenticationService.CurrentUserUid;

          
                if (!string.IsNullOrEmpty(currentUserUid))
                {
                 
                    var firebaseClient = new FirebaseClient("https://xamarin-weatherapp-default-rtdb.firebaseio.com");

                   
                    var favoritesNode = firebaseClient.Child("users").Child(currentUserUid).Child("favorites");

                  
                    var citiesSnapshot = await favoritesNode.OnceSingleAsync<Dictionary<string, Dictionary<string, object>>>();

            
                    var cityNames = citiesSnapshot.Values.Select(city => (string)city["Name"]).ToList();

            
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