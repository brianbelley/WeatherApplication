using Firebase.Auth;
using Firebase.Auth.Providers;
using Newtonsoft.Json;
using ProjectWeatherApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ProjectWeatherApp
{
    public partial class MainPage : ContentPage
    {
        private const string FirebaseApiKey = "AIzaSyAYcA1NjGz9zrlbaheOWZqn1SIee5H4bn0";
        private const string SignInUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;

            var authService = new AuthenticationService();

            var authToken = await authService.Login(email, password);
            if (!string.IsNullOrEmpty(authToken))
            {
                // Navigate to HomePage or perform other actions
                await Navigation.PushAsync(new TabbedWeatherPage());
            }
        }

        async void OnSignupButtonClicked(object sender, EventArgs e)
        {
            // Navigate to the signup page
            await Navigation.PushAsync(new SignupPage());
        }
    }

    public class FirebaseAuthErrorResponse
    {
        public FirebaseAuthError Error { get; set; }
    }

    public class FirebaseAuthError
    {
        public string Message { get; set; }
    }
}

