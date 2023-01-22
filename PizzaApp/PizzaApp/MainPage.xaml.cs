using Newtonsoft.Json;
using PizzaApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace PizzaApp
{
    public partial class MainPage : ContentPage
    {

        List<Pizza> Pizzas;
        List<string> pizzasFav = new List<string>();
        enum e_tri
        {
            TRI_AUCUN,
            TRI_NOM,
            TRI_PRIX,
            TRI_FAV
        }

        e_tri tri = e_tri.TRI_AUCUN;

        // Application.Current.Properties ( pour accéder aux User Settings)
        //KEY_TRI est une chaine de carcactère pour savoir de quoi on parle
        const string KEY_TRI = "tri";
        const string KEY_FAV = "fav";

        // le filename du stockage (le chemin du stockage)
        string tempJsonFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tempJsonFileName");
        string jsonFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "jsonFileName");

        public MainPage()
        {
            InitializeComponent();
            /*var platform = DeviceInfo.Platform;
            if(platform == DevicePlatform.Android)
            {
                Xamarin.Forms.NavigationPage.SetHasNavigationBar(this, false);
            }*/
            On<Xamarin.Forms.PlatformConfiguration.iOS>().SetUseSafeArea(true);

            LoadFavList();
            /*pizzasFav.Add("4 fromages");
            pizzasFav.Add("indienne");
            pizzasFav.Add("tartiflette");*/

            if (Xamarin.Forms.Application.Current.Properties.ContainsKey(KEY_TRI))
            {
                tri = (e_tri)Xamarin.Forms.Application.Current.Properties[KEY_TRI];
                sortButton.Source = GetImageSourceButton(tri);
            }

            const string URL = "https://drive.google.com/uc?export=download&id=1arQ9LY4Hxj32E64eHtrU6dG7nVeISc30";

            ListView.RefreshCommand = new Command((obj) =>
            {
                DownlaodData((Pizzas) => {
                    if (Pizzas != null)
                    {
                        ListView.ItemsSource = GetPizzaCells(GetPizzasTrie(tri, Pizzas), pizzasFav);
                    }
                    //ListView.ItemsSource = GetPizzasTrie(tri, Pizzas);
                     ListView.IsRefreshing = false;
                 }, URL);
            });

            ListView.IsVisible = false;
            waitLayout.IsVisible = true;

            // 
            if (File.Exists(jsonFileName))
            {
                string pizzaJson = File.ReadAllText(jsonFileName);
                if (!string.IsNullOrEmpty(pizzaJson))
                {
                    Pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzaJson);
                    ListView.ItemsSource = GetPizzaCells(GetPizzasTrie(tri, Pizzas), pizzasFav);
                    ListView.IsVisible = true;
                    waitLayout.IsVisible = false;
                }
                
            }


            // Appel Downlaod data
            DownlaodData((Pizzas) => {
                if(Pizzas != null)
                {
                    ListView.ItemsSource = GetPizzaCells(GetPizzasTrie(tri, Pizzas), pizzasFav);
                }
                ListView.IsVisible = true;
                waitLayout.IsVisible = false;
            }, URL);

            //ListView.ItemsSource = Pizzas;

        }

        public void DownlaodData(Action<List<Pizza>> action, string url)
        {
            

            using (var webClient = new WebClient())
            {
                
                    // thrhead Main (UI)
                    //pizzaJson = webClient.DownloadString(url);
                    webClient.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                    {
                        Exception ex = e.Error;
                        if(ex == null)
                        {
                            // trhead réseau
                            File.Copy(tempJsonFileName, jsonFileName, true);
                            string pizzaJson = File.ReadAllText(jsonFileName);
                            Pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzaJson);

                            // invoke du Main Thread (UI)
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                action.Invoke(Pizzas);
                            });
                        }
                        else
                        {
                            // trhead réseau
                            // invoke du Main Thread (UI)
                            Device.BeginInvokeOnMainThread( async () =>
                            {
                                await DisplayAlert("Erreur", "Une erreur de réseau s'est produite " + ex.Message, "OK");
                                action.Invoke(null);
                            });
                        }
                    };
                //webClient.DownloadStringAsync(new Uri(url));
                webClient.DownloadFileAsync(new Uri(url), tempJsonFileName);
                
            }
        }

        private void SortButtonClicked(object sender, EventArgs e)
        {
            if(tri == e_tri.TRI_AUCUN)
            {
                tri = e_tri.TRI_NOM;
            }
            else if (tri == e_tri.TRI_NOM)
            {
                tri = e_tri.TRI_PRIX;
            }
            else if(tri == e_tri.TRI_PRIX)
            {
                tri = e_tri.TRI_FAV;
            }
            else
            {
                tri = e_tri.TRI_AUCUN;
            }

            sortButton.Source = GetImageSourceButton(tri);
            //ListView.ItemsSource = GetPizzasTrie(tri, Pizzas);
            ListView.ItemsSource = GetPizzaCells(GetPizzasTrie(tri, Pizzas), pizzasFav);

            Xamarin.Forms.Application.Current.Properties[KEY_TRI] = (int)tri;
            Xamarin.Forms.Application.Current.SavePropertiesAsync();
        }

        private string GetImageSourceButton(e_tri t)
        {
            switch (t)
            {
                case e_tri.TRI_NOM:
                    return "sort_nom.png";
                case e_tri.TRI_PRIX:
                    return "sort_prix.png";
                case e_tri.TRI_FAV:
                    return "sort_fav.png";
            }

            return "sort_none.png";
        }

        private List<Pizza> GetPizzasTrie(e_tri t, List<Pizza> lp)
        {
            if(lp == null)
            {
                return null;
            }

            switch (t)
            {
                case e_tri.TRI_NOM:
                case e_tri.TRI_FAV:
                {
                        List<Pizza> ret = new List<Pizza>(lp);
                        ret.Sort((p1, p2) =>
                        {
                            return p1.Titre.CompareTo(p2.Titre);
                        });
                        return ret;
                    }
                case e_tri.TRI_PRIX:
                    {
                        List<Pizza> ret = new List<Pizza>(lp);
                        ret.Sort((p1, p2) =>
                        {
                            return p2.Prix.CompareTo(p1.Prix);
                        });
                        return ret;
                    }
            }

            return lp;
        }

        private void OnfavPizzaChanged(PizzaCell pizzaCells) {

            bool isFavList = pizzasFav.Contains(pizzaCells.pizza.Nom);

            if(pizzaCells.isFavorite && !isFavList) 
            { 
                pizzasFav.Add(pizzaCells.pizza.Nom);
            }
            else if(!pizzaCells.isFavorite && isFavList)
            {
                pizzasFav.Remove(pizzaCells.pizza.Nom);
            }

            SaveFavList();
        }

        private List<PizzaCell> GetPizzaCells(List<Pizza> p, List<string> f)
        {
            List<PizzaCell> ret = new List<PizzaCell>();

            if(p == null) { return ret; }

            foreach(Pizza pizza in p)
            {
                bool isFav = f.Contains(pizza.Nom);
                if(tri == e_tri.TRI_FAV)
                {
                    if (isFav)
                    {
                        ret.Add(new PizzaCell { pizza = pizza, isFavorite = isFav, favChangedAction = OnfavPizzaChanged });

                    }
                }
                else
                {
                    ret.Add(new PizzaCell { pizza = pizza, isFavorite = isFav, favChangedAction = OnfavPizzaChanged });
                }
            }

            return ret;
        }

        private void SaveFavList() {
            string json = JsonConvert.SerializeObject(pizzasFav);
            Xamarin.Forms.Application.Current.Properties[KEY_FAV] = json;
            Xamarin.Forms.Application.Current.SavePropertiesAsync();
        }
        private void LoadFavList() {
            if (Xamarin.Forms.Application.Current.Properties.ContainsKey(KEY_FAV))
            {
                string json = Xamarin.Forms.Application.Current.Properties[KEY_FAV].ToString();
                pizzasFav = JsonConvert.DeserializeObject<List<string>>(json);
            }
        }


    }
}
