using Firebase.Database;
using Firebase.Auth;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static System.Net.WebRequestMethods;
using Firebase.Auth.Providers;
using ProjectWeatherApp.Model;

namespace ProjectWeatherApp
{
    public partial class SignupPage : ContentPage
    {

        private const string FirebaseApiKey = "AIzaSyAYcA1NjGz9zrlbaheOWZqn1SIee5H4bn0";
        private const string SignupUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signUp?key=";

        private FirebaseClient firebaseClient;

        public SignupPage()
        {
            InitializeComponent();
            firebaseClient = new FirebaseClient("https://xamarin-weatherapp-default-rtdb.firebaseio.com/");
        }

        async void OnSignupButtonClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;

            try
            {
                var authService = new AuthenticationService();

                // Attempt to sign up the user
                var authToken = await authService.Signup(email, password);

                // Save user data to the Realtime Database
                await SaveUserDataToDatabase(email); // You can add more user data here

                await DisplayAlert("Signup Success", "Your account has been created", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Signup Error", ex.Message, "OK");
            }
        }

        async Task SaveUserDataToDatabase(string email)
        {
            try
            {
                var userData = new
                {
                    email,
                    // Add any additional user data you want to save
                };

                // Create a new entry in the 'users' node with UID as the key
                await firebaseClient.Child("users").Child(AuthenticationService.CurrentUserUid).PutAsync(userData);
                Console.WriteLine("User data saved to database.");
            }
            catch (Exception ex)
            {
                // Handle database save errors
                Console.WriteLine($"Failed to save user data: {ex.Message}");
            }
        }

        async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            // Navigate back to the login page
            await Navigation.PopAsync();
        }

    }
}
