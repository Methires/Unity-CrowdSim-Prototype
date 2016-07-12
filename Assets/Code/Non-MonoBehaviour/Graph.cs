using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Non_MonoBehaviour
{
    class Graph
    {
        public class Node
        {
            public string AnimationName { get; set; }
            public int Id { get; set; }
            public List<string> Actors { get; set; }
            public string Name { get; set; }

            public Node()
            {
                AnimationName = "default";
                Id = -1;
                Actors = new List<string>();
                Name = "default";
            }

            public Node(string animationName, int id, List<string> actors, string name = "default_name")
            {
                AnimationName = animationName;
                Id = id;
                Actors = actors;
                Name = name;
            }
        }

        public class Connection
        {
            public float Probability
            {
                get
                {
                    return Probability;
                }
                set
                {
                    Probability = value > 1.0f ? 1.0f : value;
                    Probability = value < 0.0f ? 0.0f : value;
                }
            }
            public int ParentId { get; set; }
            public int ChildId { get; set; }

            public Connection()
            {
                Probability = 0.0f;
                ParentId = -1;
                ChildId = -1;
            }

            public Connection(float probability, int parentId, int childId)
            {
                Probability = probability;
                ParentId = parentId;
                ChildId = childId;
            }
        }

        public struct GraphElement
        {
            public Node SingleNode;
            public List<Connection> Conections;

            public GraphElement(Node node, List<Connection> connections)
            {
                SingleNode = node;
                Conections = connections;
            }
            public GraphElement(Node node)
            {
                SingleNode = node;
                Conections = null;
            }
        }

    }
}
