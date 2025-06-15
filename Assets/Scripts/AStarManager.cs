using Utils;
using System.Collections.Generic;
using UnityEngine;

public class AStarManager : MonoBehaviour {

    public static AStarManager instance { get; private set; }
    private AStarNode[] nodes;

    void Awake() {
        instance = this;
        nodes = FindObjectsByType<AStarNode>(FindObjectsSortMode.None);
    }

    /**
     * generates the shortest path from the start to the end node
     * returns the list of the path if it exists,
     * or return null otherwise.
     */
    public List<AStarNode> GeneratePath(AStarNode start, AStarNode end) {
        PriorityQueue<AStarNode, float> queue = new PriorityQueue<AStarNode, float>();

        foreach (AStarNode node in nodes)
            node.gScore = float.MaxValue;

        start.gScore = 0;
        start.hScore = Vector3.Distance(start.transform.position, end.transform.position);
        start.cameFrom = null;

        queue.Enqueue(start, start.FScore());

        while (queue.Count > 0) {
            AStarNode node = queue.Dequeue();

            if (node == end)
                return node.GeneratePathToCurrentNode();

            foreach (AStarNode connection in node.GetConnections()) {
                if (connection != node.cameFrom) {
                    float gScore = node.gScore + Vector3.Distance(node.transform.position, connection.transform.position);
                    float hScore = Vector3.Distance(connection.transform.position, end.transform.position);

                    if (gScore + hScore < connection.FScore()) {
                        connection.gScore = gScore;
                        connection.hScore = hScore;
                        connection.cameFrom = node;
                        queue.Enqueue(connection, connection.FScore());
                    }
                }
            }
        }

        return null;
    }
}
