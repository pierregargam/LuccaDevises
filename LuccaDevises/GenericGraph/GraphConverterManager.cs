using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ConversionGraph
{
    public class GraphConverterManager
    {

        public Graph graph;

        public int? ConvertCurrenciesFromFile(string fileName, StringBuilder errorMessage)
        {
            int? convertedValue = null;

            //Creation of the graph from the datas
            ConvertFileToGraph(fileName, errorMessage);

            //Use the graph to find the shortest way to convert
            if (graph.FindShortestPath())
                //Make the conversion
                convertedValue = graph.Convert();
            else
                //if we can't find a path, display a message
                errorMessage.Append(Resource.InconvertibleCurrency);

            return convertedValue;
        }

        /// <summary>
        /// Read the file and create the graph from it
        /// </summary>
        /// <param name="fileName">file where we have to read the datas</param>
        /// <param name="errorMessage"></param>
        /// <exception cref="InvalidDataException">Thrown when some line of the file aren't valid.</exception>
        private void ConvertFileToGraph(string fileName, StringBuilder errorMessage)
        {
            var datas = File.ReadAllLines(fileName);
            //There should be at least 3 line
            if (datas.Length < 3)
            {
                errorMessage.AppendLine(Resource.NotEnoughRows);
            }


            graph = new Graph();

            //The first line should describe the currency conversion we want to do
            if (!LoadConversionLine(datas[0]))
            {
                errorMessage.AppendLine(Resource.Line1Usage);
            }

            //The second line should describe the number of exchange rate we get
            if (int.TryParse(datas[1], out int nbSupposedRow))
            {
                if (nbSupposedRow != datas.Length - 2)
                {
                    errorMessage.AppendFormat(Resource.UnexpectedNumberOfRows, datas.Length - 2, nbSupposedRow);
                }
            }
            else
                errorMessage.AppendLine(Resource.Line2Usage);

            //The next lines should each describe an exchange rate
            for (int i = 2; i < datas.Length; i++)
            {
                if (!LoadExchangeData(datas[i]))
                {
                    errorMessage.AppendFormat(Resource.Line3Usage, i + 1);
                }
            }

            //if we got errormessage, the file isn't valid, so we should not use it
            if (errorMessage.Length > 0)
                throw (new InvalidDataException());
        }

        /// <summary>
        /// Check if the conversionLine is valid and add it to the graph
        /// </summary>
        /// <param name="conversionLine">Describe the wanted exchange</param>
        /// <returns>return true if the line validate the requirements</returns>
        private bool LoadConversionLine(string conversionLine)
        {
            var lineSplit = conversionLine.Split(";");
            if (lineSplit.Length != 3)
                return false;

            if (!IsCurrency(lineSplit[0]) || !IsCurrency(lineSplit[2]))
                return false;

            if (int.TryParse(lineSplit[1], out int valueToConvert))
            {
                //we can't have an exchange rate negative or zero
                if (valueToConvert <= 0)
                    return false;
            }
            else
                return false;

            //Once the graph is created with nodes, we can add the data about the calculation to be done
            //we put the currency into upper letter to avoid duplications
            AddFinalConversionDatas(lineSplit[0].ToUpper(), lineSplit[2].ToUpper(), valueToConvert);

            return true;
        }


        /// <summary>
        /// Check if the  line of the exhange rate is valid and add it to the the graph
        /// </summary>
        /// <param name="conversionLine">Line to load</param>
        /// <returns>return true if the line validate the requirements</returns>
        private bool LoadExchangeData(string exchangeLine)
        {
            //line format : starting currency; arrival currency; exchange rate
            var lineSplit = exchangeLine.Split(";");
            if (lineSplit.Length != 3)
                return false;

            if (!IsCurrency(lineSplit[0]) || !IsCurrency(lineSplit[1]))
                return false;

            //exchange rate decimal are using a "." instead of a ","
            if (decimal.TryParse(lineSplit[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal rate))
            {
                if (decimal.Round(rate, 4) <= 0)
                    return false;
            }
            else
                return false;

            //we put the currency into upper letter to avoid duplications
            graph.AddPathWithReversal(lineSplit[0].ToUpper(), lineSplit[1].ToUpper(), rate);

            return true;
        }

        /// <summary>
        /// add datas about the calculation to do to the graph
        /// </summary>
        /// <param name="graph">Graph to update</param>
        /// <param name="source">Currency source of the conversion</param>
        /// <param name="destination">Currency destination of the conversion</param>
        /// <param name="amount">amount to convert</param>
        private void AddFinalConversionDatas(string source, string destination, decimal amount)
        {
            graph.SourceNode = graph.AddNode(source);
            graph.DestinationNode = graph.AddNode(destination);
            graph.AmountToConvert = amount;
        }

        /// <summary>
        /// Verify if the string in parameters validate the requirements to be used as a currency
        /// </summary>
        /// <param name="supposedCurrency"></param>
        /// <returns>Return true id the supposedCurrency can be considered as a currency</returns>
        public bool IsCurrency(string supposedCurrency)
        {
            //Currencies are described by 3 letters
            if (supposedCurrency.Length != 3)
                return false;

            if (supposedCurrency.ToCharArray().Any(c => !char.IsLetter(c)))
                return false;

            return true;
        }

        public void SetNewAmount(decimal amount)
        {
            graph.AmountToConvert = amount;
        }

        /// <exception cref="ArgumentException"></exception>
        public void SetNewStartCurrency(string startCurrency)
        {
            var node = graph.Nodes.FirstOrDefault(n => n.Currency == startCurrency);
            if (node != null)
                graph.SourceNode = node;
            else
                throw new ArgumentException("Cette valeur n'existe pas dans les données d'origine.");
        }

        /// <exception cref="ArgumentException"></exception>
        public void SetNewFinalCurrency(string finalCurrency)
        {
            var node = graph.Nodes.FirstOrDefault(n => n.Currency == finalCurrency);
            if (node != null)
                graph.DestinationNode = node;
            else
                throw new ArgumentException("Cette valeur n'existe pas dans les données d'origine.");
        }
    }
}
