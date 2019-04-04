using System;
using System.Text;
using System.IO;
using System.Linq;
using ConversionGraph;

namespace LuccaDevisesMultiUsage
{
    /// <summary>
    /// Converter class will read the parameters to retrieve a file
    /// The file will describe a conversion that as to be done and give the available exhange between currencies.
    /// Il will use the graph theory to describe the datas in objects and to search the shortest path between a given currency and wanted one.
    /// It will display the result of the conversion if possible or it will display a message describing the trouble it faced.
    /// </summary>
    public class Converter
    {
        GraphConverterManager GraphManager;

        public void Start()
        {
            Console.WriteLine("Entrer le chemin du fichier à utiliser.");
            var path = GetPath();

            //will be used to display detected error to the user.
            StringBuilder errorMessage = new StringBuilder();
            //if the input datas are incorrect, an exception will be thrown describing what we have to correct.
            //unhandled errors will be displayed on the console.
            try
            {
                GraphManager = new GraphConverterManager();
                var result = GraphManager.ConvertCurrenciesFromFile(path, errorMessage);
                if (result.HasValue)
                    Console.WriteLine(result.Value);
            }
            catch (Exception e)
            {
                errorMessage.Append(e.ToString());
            }
            finally
            {
                if (errorMessage.Length > 0)
                {
                    Console.WriteLine(errorMessage.ToString());
                }
            }

            WaitCommand();
        }

        private void WaitCommand()
        {
            Console.WriteLine("Pour définir un nouveau montant, tappez M suivi du nouveau montant. Exemple :");
            Console.WriteLine("M 2500");
            Console.WriteLine("Pour définir une nouvelle devise de de départ, tappez D suivi de la devise. Exemple :");
            Console.WriteLine("D USD");
            Console.WriteLine("Pour définir une nouvelle devise de d'arrivée, tappez A suivi de la devise. Exemple :");
            Console.WriteLine("A EUR");
            Console.WriteLine("Pour définir un nouveau fichier de calcul, tappez F suivi du chemin du fichier Exemple :");
            Console.WriteLine("F D:/Fichier de conversion");
            Console.WriteLine("Pour lancer une conversion avec les paramètres actuels, tappez C.");
            Console.WriteLine("Pour quitter, tappez Q.");

            bool wait = true;
            while(wait)
            {
                var commandString = Console.ReadLine();
                switch (commandString.Substring(0, 1))
                {
                    case "M":
                        if (decimal.TryParse(commandString.Substring(2), out decimal amount))
                        {
                            GraphManager.SetNewAmount(amount);
                            Console.WriteLine($"Nouveau montant : {amount}");
                        }
                        else
                            Console.WriteLine("Dois-je expliquer à quoi ressemble un montant?");
                        break;
                    case "D":
                        var startCurrency = commandString.Substring(2);
                        if (GraphManager.IsCurrency(startCurrency))
                        {
                            try
                            {
                                GraphManager.SetNewStartCurrency(startCurrency);
                                Console.WriteLine($"Nouvelle devise : {startCurrency}");
                            }
                            catch (ArgumentException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        else
                            Console.WriteLine("Trois lettres pour une devise. Il y a un exemple au dessus.");
                        break;
                    case "A":
                        var endCurrency = commandString.Substring(2);
                        if (GraphManager.IsCurrency(endCurrency))
                        {
                            try
                            { 
                                GraphManager.SetNewFinalCurrency(endCurrency);
                                Console.WriteLine($"Nouvelle devise : {endCurrency}");
                            }
                            catch (ArgumentException e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        else
                            Console.WriteLine("Trois lettres pour une devise. Il y a un exemple au dessus.");
                        break;
                    case "F":
                        StringBuilder errorMessage = new StringBuilder();
                        var result = GraphManager.ConvertCurrenciesFromFile(commandString.Substring(2), errorMessage);
                        if (result.HasValue)
                            Console.WriteLine(result.Value);
                        Console.WriteLine(errorMessage.ToString());
                        break;
                    case "C":
                        if (GraphManager.graph.FindShortestPath())
                            Console.WriteLine(GraphManager.graph.Convert());
                        else
                            Console.WriteLine("Il n'y a pas de conversion possible entre ces deux montants.");
                        break;
                    case "Q":
                        wait = false;
                        break;
                    default:
                        Console.WriteLine("Il semble y avoir un problème de compréhension entre vous et moi.");
                        break;
                }
            }
        }


        private bool CheckPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Console.WriteLine(Resource.UnvalidFileParameter);
                return false;
            }

            return true;
        }

        private string GetPath()
        {
            while (true)
            {
                var path = Console.ReadLine();

                if (CheckPath(path))
                    return path;
            }
        }

    }
}
