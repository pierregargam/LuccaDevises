using System;
using System.Text;
using System.IO;
using System.Linq;
using ConversionGraph;

namespace LuccaDevises
{
    /// <summary>
    /// Converter class will read the parameters to retrieve a file
    /// The file will describe a conversion that as to be done and give the available exhange between currencies.
    /// Il will use the graph theory to describe the datas in objects and to search the shortest path between a given currency and wanted one.
    /// It will display the result of the conversion if possible or it will display a message describing the trouble it faced.
    /// </summary>
    public class Converter
    {
        /// <summary>
        /// Read the conversion file from the path in parameter and calculate the wanted conversion from a currency to another.
        /// </summary>
        /// <param name="args">Arguments from the command line
        /// Should be a path to a file defining the conversion to do.</param>
        /// <returns></returns>
        public void ConvertCurrenciesFromDataFile(string[] args)
        {
            //will be used to display detected error to the user.
            StringBuilder errorMessage = new StringBuilder();

            //if the input datas are incorrect, an exception will be thrown describing what we have to correct.
            //unhandled errors will be displayed on the console.
            try
            {
                string fileName = ReadArgs(args, errorMessage);

                var GraphConverterManager = new GraphConverterManager();
                var result = GraphConverterManager.ConvertCurrenciesFromFile(fileName, errorMessage);
                if(result.HasValue)
                    Console.WriteLine(result.Value);
            }
            catch(Exception e)
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
        }


        /// <summary>
        /// Check if the args have the expected format.
        /// expected format : path to a file.
        /// <param name="args">Arguments from the command line</param>
        /// <param name="errorMessage">Allow to prepare message in order to display it</param>
        /// <returns>the file path from yhe arguments</returns>
        /// <exception cref="ArgumentException">Thrown when there is no argument to create a file path.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the path describe a file that does not exist.</exception>
        private string ReadArgs(string[] args, StringBuilder errorMessage)
        {
            if (args.Length == 0)
            {
                errorMessage.AppendLine(Resource.NoFileParameter);
                throw (new ArgumentException());
            }
            //We will handle manually the case the user put a path with spaces in it without using quote.
            //So we have to aggregate all the arguments into a single pathfile.
            else
            {
                string fileName = args.Aggregate((i, j) => i + " " + j);

                if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                {
                    errorMessage.AppendLine(Resource.UnvalidFileParameter);
                    throw (new FileNotFoundException());
                }
                return fileName;
            }
        }
       
    }
}
