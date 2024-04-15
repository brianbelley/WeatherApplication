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
    public partial class HistoryPage : ContentPage
    {

        // Static list to store search history
        private static List<string> SearchHistory { get; set; } = new List<string>();

        // Constructor for HistoryPage accessed from tab
        public HistoryPage()
        {
            InitializeComponent();
        }

        // Constructor for HistoryPage accessed from navigation
        public HistoryPage(List<string> searchHistory)
        {
            InitializeComponent();

            // Set the search history from the parameter
            SearchHistory = searchHistory;
            LoadSearchHistory();
        }

        // Method to load search history
        private void LoadSearchHistory()
        {
            // Set the search history as the item source for the ListView
            historyListView.ItemsSource = SearchHistory;
        }

        // Method to add search term to history
        public static void AddToSearchHistory(string searchTerm)
        {
            // Add the search term to the search history list
            SearchHistory.Add(searchTerm);
        }

        // Override the OnAppearing method to refresh the search history
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Load search history if accessed from tab
            if (SearchHistory.Count > 0)
            {
                LoadSearchHistory();
            }
        }
    }
}