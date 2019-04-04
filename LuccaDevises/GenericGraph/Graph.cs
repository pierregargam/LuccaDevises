using System;
using System.Collections.Generic;
using System.Linq;

namespace ConversionGraph
{
    /// <summary>
    /// Represent the datas of a graph
    /// </summary>
    public class Graph
    {
        public List<Node> Nodes;
        public List<Path> Paths;

        /// <summary>
        /// starting node for the path finding
        /// </summary>
        public Node SourceNode;

        /// <summary>
        /// ending node for the path finding
        /// </summary>
        public Node DestinationNode;

        /// <summary>
        /// amount that will be converted by the chosen path
        /// </summary>
        public decimal AmountToConvert;

        public Graph()
        {
            Nodes = new List<Node>();
            Paths = new List<Path>();
        }

        /// <summary>
        /// add a node to the Graph if it not already exists
        /// </summary>
        /// <param name="nodeName"></param>
        public Node AddNode(string nodeName)
        {
            Node node;
            if (Nodes.Any(n => n.Currency == nodeName))
            {
                node = Nodes.First(n => n.Currency == nodeName);
            }
            else
            {
                node = new Node(nodeName);
                Nodes.Add(node);
            }

            return node;
        }

        /// <summary>
        /// Register a weigthed path between two nodes and the revseral one
        /// Create the nodes if they don't exist.
        /// </summary>
        /// <param name="startNodeName">starting node of the path</param>
        /// <param name="endNodeName">ending node of the path</param>
        /// <param name="exchangeRate">rate to apply to convert from the start to the end</param>
        public void AddPathWithReversal(string startNodeName, string endNodeName, decimal exchangeRate)
        {
            if (exchangeRate == 0)
                throw new ArgumentException("Le taux de change de peut pas être 0");

            Node startNode = Nodes.FirstOrDefault(n => n.Currency == startNodeName) ?? AddNode(startNodeName);
            Node endNode = Nodes.FirstOrDefault(n => n.Currency == endNodeName) ?? AddNode(endNodeName);

            Path path = new Path()
            {
                StartNode = startNode,
                EndNode = endNode,
                ExchangeRate = exchangeRate
            };
            Paths.Add(path);
            //we link the created path to the starting node to make them easier to find
            startNode.Paths.Add(path);

            Path reversePath = new Path()
            {
                StartNode = endNode,
                EndNode = startNode,
                ExchangeRate = decimal.Round(1 / exchangeRate, 4)
            };
            Paths.Add(reversePath);
            //we link the created path to the starting node to make them easier to find
            endNode.Paths.Add(reversePath);
        }

        /// <summary>
        /// find the shorest path between the source and the destionation
        /// </summary>
        public bool FindShortestPath()
        {
            //reset the nodes for a new calculation
            Nodes.ForEach(n => { n.Depth = 0; n.Parent = null; });
            
            Stack<Node> queue = new Stack<Node>();
            queue.Push(SourceNode);
            SourceNode.Depth = 1; //the frist node of the path

            Node current;
            while (queue.Count > 0)
            {
                current = queue.Pop();
                //if we get to the destination, that only mean there is a path to the destination, but we have to continue to pop the queue to look for a shorter path
                if (current != DestinationNode)
                {
                    //we check all paths from the current node to see if there is an accessible node that hasn't been visited or that has an higher depth and for which we found a shortest path to access
                    current.Paths
                        .Where(p => p.EndNode.Depth == 0 || p.EndNode.Depth > current.Depth + 1)
                        .ToList()
                        .ForEach(p =>
                        {
                            p.EndNode.Depth = current.Depth + 1;
                            //we keep the parent from wich we made the path to be able to retrieve the full path
                            p.EndNode.Parent = current;
                            queue.Push(p.EndNode);
                        });
                }
            }
            //if the depth of the destination node is 0, then we were unable to find any path.
            //we can't make the conversion
            return DestinationNode.Depth != 0;
        }

        /// <summary>
        /// convert the amount by traveling along the path from the beginning to the end
        /// </summary>
        /// <returns></returns>
        public int Convert()
        {
            //create the route by finding the parent of each node. starting from the end.
            //each node only know is parent of the shortest way
            Stack<Path> route = new Stack<Path>();
            Node currentNode = DestinationNode;
            while (currentNode.Parent != null)
            {
                //find the path between the node and his parent - starting FROM the PARENT
                route.Push(currentNode.Parent.Paths.First(p => p.EndNode.Currency == currentNode.Currency));
                currentNode = currentNode.Parent;
            }

            //the path is created, now we can go through it while we convert the amount
            decimal currentAmount = AmountToConvert;
            while (route.Count > 0)
            {
                var path = route.Pop();
                var rate = path.ExchangeRate;

                currentAmount = decimal.Round(currentAmount * rate, 4);
            }

            return (int)Math.Round(currentAmount);
        }
    }
}
