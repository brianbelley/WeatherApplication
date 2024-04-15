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


        private static List<string> SearchHistory { get; set; } = new List<string>();


        public HistoryPage()
        {
            InitializeComponent();
        }


        public HistoryPage(List<string> searchHistory)
        {
            InitializeComponent();


            SearchHistory = searchHistory;
            LoadSearchHistory();
        }


        private void LoadSearchHistory()
        {

            historyListView.ItemsSource = SearchHistory;
        }


        public static void AddToSearchHistory(string searchTerm)
        {

            SearchHistory.Add(searchTerm);
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (SearchHistory.Count > 0)
            {
                LoadSearchHistory();
            }
        }
    }
}