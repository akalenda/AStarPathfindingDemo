using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstarDemo
{
    class Astar
    {
        Node currNode;
        Node[,] map;
        Int32 width;
        Int32 height;
        Int32 strtX;
        Int32 strtY;
        Int32 currX;
        Int32 currY;
        Int32 destX;
        Int32 destY;
        public delegate void DataFeed(Int32 x, Int32 y, Char type, Int32 data, Char direction);
        DataFeed callback;
        List<Node> openList;
        
        // CONSTRUCTOR; array must match the given dimensions, and is used to provide
        // the cost of traversing points on the map. Use Int32.MaxValue for impassible points.
        public Astar(Int32 width, Int32 height, Int32[,] traversalCosts)
        {
            this.width = width;
            this.height = height;
            map = new Node[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    map[x, y] = new Node(x, y, traversalCosts[x, y]);
            openList = new List<Node>();
        }

        // Remove the given node from the open list and mark it closed
        private void close(Node n)
        {
            n.isOpen = false;
            n.isClosed = true;
            openList.Remove(n);
        }

        // Sets the algorithm to run from a new start to a new end position
        // The DataFeed callback lets the algorithm (optionally) provide feedback to the host program
        public void navFromTo(Int32 startX, Int32 startY, Int32 destinationX, Int32 destinationY, DataFeed callback)
        {
            // Reset old nav data
            foreach (Node n in map)
            {
                n.isClosed = false;
                n.isOpen = false;
                n.parent = null;
            }
            
            // Save new navigation data
            strtX = startX;
            strtY = startY;
            destX = destinationX;
            destY = destinationY;
            this.callback = callback;
            
            // Add starting node to open_list
            open(map[strtX, strtY], null, ' ');
        }

        // Add the given node to the open list with the given parent
        private void open(Node n, Node parent, Char direction)
        {
            if (n.isOpen || n.isClosed)
                return;
            if (n.traversalCost == Int32.MaxValue)
            {
                n.etaCost = Int32.MaxValue;
                n.totalCost = Int32.MaxValue;
                n.isClosed = true;
                return;
            }
            n.parent = parent;
            if (parent != null)
                n.arrivalCost = parent.arrivalCost + parent.traversalCost;
            n.etaCost = Math.Abs(destX - currX) + Math.Abs(destY - currY) + n.traversalCost;
            n.totalCost = n.arrivalCost + n.etaCost;
            openList.Add(n);
            n.isOpen = true;
            callback(n.x, n.y, 'o', n.totalCost, direction);
        }

        // Progresses the algorithm forward one step
        // Returns a linked list of x-y value pairs delineating a complete path if it is found
        // Returns an empty linked list if no such path is possible
        // Returns null if there are steps remaining
        public LinkedList<Int32[]> step()
        {
            // While there's stuff in the open list
            if (0 < openList.Count)
            {
                // Set current node to be the lowest-cost node from open list
                openList.Sort();
                currNode = openList[0];
                currX = currNode.x;
                currY = currNode.y;

                // Is it the goal?
                if (currX == destX && currY == destY)
                {
                    // Path is complete
                    LinkedList<Int32[]> path = new LinkedList<Int32[]>();
                    Int32[] point;
                    currNode = currNode.parent;
                    while (currNode.parent != null)
                    {
                        point = new Int32[2];
                        point[0] = currNode.x;
                        point[1] = currNode.y;
                        path.AddFirst(point);
                        currNode = currNode.parent;
                    }
                    return path;
                }
                else
                {
                    // Close current node
                    close(currNode);
                    callback(currX, currY, 'c', currNode.totalCost, ' ');

                    // Open each adjacent node
                    if (currNode.y > 0) // north
                        open(map[currNode.x, currNode.y - 1], currNode, '^');
                    if (currNode.x + 1 < width) // east
                        open(map[currNode.x + 1, currNode.y], currNode, '>');
                    if (currNode.y + 1 < height) // south
                        open(map[currNode.x, currNode.y + 1], currNode, 'v');
                    if (currNode.x > 0) // west
                        open(map[currNode.x - 1, currNode.y], currNode, '<');
                }
                return null;
            }
            // Unable to find a path!
            return new LinkedList<Int32[]>();
        }

        // Alters the traversal cost of the given point
        public void updateTraversal(Int32 x, Int32 y, Int32 newCost)
        {
            map[x, y].traversalCost = newCost;
        }

        // Convenience class containing all the data pertinent to a particular position on the grid
        public class Node : IComparable
        {
            public Boolean isOpen;
            public Boolean isClosed;
            public Node parent;
            public Int32 arrivalCost;
            public Int32 traversalCost;
            public Int32 etaCost;
            public Int32 totalCost;
            public Int32 x;
            public Int32 y;

            // CONSTRUCTOR
            public Node(Int32 x, Int32 y, Int32 travelCost)
            {
                this.x = x;
                this.y = y;
                this.traversalCost = travelCost;
                isClosed = false;
                isOpen = false;
            }

            public int Compare(object o1, object o2)
            {
                Node n1 = (Node)o1;
                Node n2 = (Node)o2;

                if (n1.totalCost < n2.totalCost)
                    return -1;
                if (n1.totalCost > n2.totalCost)
                    return 1;
                return 0;
            }

            public int CompareTo(object o)
            {
                Node n = (Node)o;

                if (totalCost < n.totalCost)
                    return -1;
                if (totalCost > n.totalCost)
                    return 1;
                return 0;
            }
        }
    }
}
