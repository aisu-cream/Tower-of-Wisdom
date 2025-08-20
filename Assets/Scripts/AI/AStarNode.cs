using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarNode : MonoBehaviour {

    [SerializeField] List<AStarNode> connections = new List<AStarNode>();
    [NonSerialized] public AStarNode cameFrom;

    [NonSerialized] public float gScore;
    [NonSerialized] public float hScore;

    public List<AStarNode> GetConnections() {
        return connections;
    }

    public float FScore() {
        return gScore + hScore;
    }

    public List<AStarNode> GeneratePathToCurrentNode() {
        return RecursivelyGeneratePath(new List<AStarNode>(), this);
    }

    private List<AStarNode> RecursivelyGeneratePath(List<AStarNode> list, AStarNode currNode) {
        if (currNode != null) {
            List<AStarNode> temp = RecursivelyGeneratePath(list, currNode.cameFrom);
            temp.Add(currNode);
            return temp;
        }

        return list;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        for (int i = 0; i < connections.Count; i++) {
            if (connections[i] != null)
                Gizmos.DrawLine(transform.position, connections[i].transform.position);
        }
    }
}
