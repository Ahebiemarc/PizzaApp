using PizzaApp.extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PizzaApp.Model
{

    public class Pizza
    {
        public string Nom { get; set; }
        public double Prix { get; set; }
        public string ImageUrl { get; set; }
        public string[] Ingredients { get; set; }
        public string PrixEuro { get { return Prix + "€"; } }
        public string StringIngrdients { get { return string.Join(", ", Ingredients); } }

        public string Titre { get { return Nom.FirstLetterUpperCase(); } }



        public Pizza() { }

        /*public Pizza(string nom, double prix, string[] ingredients) {
            Nom = nom;
            Prix = prix;
            Ingredients = ingredients;
        }*/
    }
}
