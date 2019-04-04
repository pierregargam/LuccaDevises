using System.Collections.Generic;

namespace ConversionGraph
{
    public class Node
    {
        public string Currency;

        //Depth from the source Node to the destinationNode
        //lower is the closer to the source
        public int Depth;

        //Parent from where we can come through the shorest route
        public Node Parent;

        /// <summary>
        /// Paths starting from the current node
        /// </summary>
        public List<Path> Paths;
        
        public Node(string currency)
        {
            Currency = currency;
            Paths = new List<Path>();
        }
    }
}
