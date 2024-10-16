using Godot;
using System;
using System.Collections.Generic;

namespace NewDuhamel.utilities
{
    /// <summary>
    /// Uses Kruskal's algorithm to find the Minimum Spanning Tree of an undirected graph structure
    /// </summary>
    public static class MinSpanningTree
    {
        public class Graph
        {

            // A class to represent a graph edge 
            public class Edge : IComparable<Edge>
            {
                public int src, dest, weight;

                // Comparator function used for sorting edges 
                // based on their weight 
                public int CompareTo(Edge compareEdge)
                {
                    return this.weight - compareEdge.weight;
                }
            }

            // A class to represent 
            // a subset for union-find 
            public class subset
            {
                public int parent, rank;
            };

            // V-> no. of vertices & E->no.of edges 
            int V, E;

            // Collection of all edges 
            public Edge[] edge { get; set; }

            // Creates a graph with V vertices and E edges 
            public Graph(int v, int e)
            {
                V = v;
                E = e;
                edge = new Edge[E];
                for (int i = 0; i < e; ++i)
                    edge[i] = new Edge();
            }

            // A utility function to find set of an element i 
            // (uses path compression technique) 
            int find(subset[] subsets, int i)
            {
                // Find root and make root as 
                // parent of i (path compression) 
                if (subsets[i].parent != i)
                    subsets[i].parent
                        = find(subsets, subsets[i].parent);

                return subsets[i].parent;
            }

            // A function that does union of 
            // two sets of x and y (uses union by rank) 
            void Union(subset[] subsets, int x, int y)
            {
                int xroot = find(subsets, x);
                int yroot = find(subsets, y);

                // Attach smaller rank tree under root of 
                // high rank tree (Union by Rank) 
                if (subsets[xroot].rank < subsets[yroot].rank)
                    subsets[xroot].parent = yroot;
                else if (subsets[xroot].rank > subsets[yroot].rank)
                    subsets[yroot].parent = xroot;

                // If ranks are same, then make one as root 
                // and increment its rank by one 
                else
                {
                    subsets[yroot].parent = xroot;
                    subsets[xroot].rank++;
                }
            }

            // The main function to construct MST 
            // using Kruskal's algorithm 
            public Edge[] KruskalMST()
            {
                // This will store the 
                // resultant MST 
                Edge[] result = new Edge[V];

                // An index variable, used for result[] 
                int e = 0;

                // An index variable, used for sorted edges 
                int i = 0;
                for (i = 0; i < V; ++i)
                    result[i] = new Edge();

                // Sort all the edges in non-decreasing 
                // order of their weight. If we are not allowed 
                // to change the given graph, we can create 
                // a copy of array of edges 
                Array.Sort(edge);

                // Allocate memory for creating V subsets 
                subset[] subsets = new subset[V];
                for (i = 0; i < V; ++i)
                    subsets[i] = new subset();

                // Create V subsets with single elements 
                for (int v = 0; v < V; ++v)
                {
                    subsets[v].parent = v;
                    subsets[v].rank = 0;
                }
                i = 0;

                // Number of edges to be taken is equal to V-1 
                while (e < V - 1)
                {

                    // Pick the smallest edge. And increment 
                    // the index for next iteration 
                    Edge next_edge = new Edge();
                   
                    //GD.Print("i++ = " + i+1);
                    //GD.Print("edge count = " + edge.Length);
                    next_edge = edge[i++];

                        int x = find(subsets, next_edge.src);
                        int y = find(subsets, next_edge.dest);

                        // If including this edge doesn't cause cycle, 
                        // include it in result and increment the index 
                        // of result for next edge 
                        if (x != y)
                        {
                            result[e++] = next_edge;
                            Union(subsets, x, y);
                        }
                }

                // Print the contents of result[] to display 
                // the built MST 
                GD.Print("Following are the edges in "
                                  + "the constructed MST");

                int minimumCost = 0;
                for (i = 0; i < e; ++i)
                {
                    GD.Print(result[i].src + " -- "
                                      + result[i].dest
                                      + " == " + result[i].weight);
                    minimumCost += result[i].weight;
                }

                GD.Print("Minimum Cost Spanning Tree: "
                                  + minimumCost);

                return result;
            }

            // Driver's Code 
            public static void Main(String[] args)
            {
                int V = 4;
                int E = 5;
                Graph graph = new Graph(V, E);

                // add edge 0-1 
                graph.edge[0].src = 0;
                graph.edge[0].dest = 1;
                graph.edge[0].weight = 10;

                // add edge 0-2 
                graph.edge[1].src = 0;
                graph.edge[1].dest = 2;
                graph.edge[1].weight = 6;

                // add edge 0-3 
                graph.edge[2].src = 0;
                graph.edge[2].dest = 3;
                graph.edge[2].weight = 5;

                // add edge 1-3 
                graph.edge[3].src = 1;
                graph.edge[3].dest = 3;
                graph.edge[3].weight = 15;

                // add edge 2-3 
                graph.edge[4].src = 2;
                graph.edge[4].dest = 3;
                graph.edge[4].weight = 4;

                // Function call 
                graph.KruskalMST();
            }
        }
    }
}
