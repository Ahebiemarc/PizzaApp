using PizzaApp.extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace PizzaApp.Model
{

    public class PizzaCell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Pizza pizza { get; set; }
        public bool isFavorite { get; set; }
        public string imageSourceFav { get { return isFavorite ? "star2.png" : "star1.png"; } }
        public ICommand favClickCommand { get; set; }
        public Action<PizzaCell> favChangedAction { get; set; }



        public PizzaCell() {
            favClickCommand = new Command((obj) => {
                Console.WriteLine("Clicked");
                isFavorite = !isFavorite;
                OnPropertyChanged("imageSourceFav");
                favChangedAction.Invoke(this);
            });
        }


        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /*public Pizza(string nom, double prix, string[] ingredients) {
            Nom = nom;
            Prix = prix;
            Ingredients = ingredients;
        }*/
    }
}
