using System;

namespace LuccaDevises
{
    class Program
    {
        static void Main(string[] args)
        {
            var converter = new Converter();
            converter.ConvertCurrenciesFromDataFile(args);
            
            Console.WriteLine("Appuyer sur une touche pour terminer.");
            Console.ReadKey();
        }
    }
}
