using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Platform : MonoBehaviour {

    // initialNode => go to nodes in order => (if repeat is true) then => go to initialNode => repeat

    #region Fields
    Rigidbody rb;
    const float eps = 0.03f;

    [SerializeField] float speed = 2f;
    [SerializeField] bool repeat;

    [SerializeField] List<Node> nodes;
    Vector3 initialPosition;
    int index;
    float waitTimer;
    #endregion

    void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        initialPosition = transform.position;
    }

    void OnDrawGizmos() {
        if (nodes.Count == 0)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, GetNodePosition(index));

        for (int i = index + 1; i < nodes.Count; i++)
            Gizmos.DrawLine(GetNodePosition(i - 1), GetNodePosition(i));
    }

    void FixedUpdate() {
        if (waitTimer > 0) {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        if (ReachedCurrentNode()) {
            if (!repeat && index == nodes.Count - 1)
                return;
            else {
                waitTimer = nodes[index].waitTime;
                IncrementIndex();
            }
        }

        if (waitTimer <= 0) {
            Vector3 currentNodePosition = GetNodePosition(index);
            Vector3 direction = (currentNodePosition - rb.position).normalized;
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        }
    }

    bool ReachedCurrentNode() {
        Vector3 currentNodePosition = GetNodePosition(index);
        float sqrDistance = (rb.position - currentNodePosition).sqrMagnitude;
        return sqrDistance <= eps;
    }

    void IncrementIndex() {
        index += 1;
        if (index >= nodes.Count)
            index = 0;
    }

    Vector3 GetNodePosition(int index) {
        if (index >= nodes.Count)
            throw new System.IndexOutOfRangeException("Node index out of range");
        return nodes[index].position + initialPosition;
    }

    [Serializable]
    class Node {
        [SerializeField] public Vector3 position;
        [SerializeField, Min(0)] public float waitTime;

        public Node(Vector3 position, float waitTime) {
            this.position = position;
            this.waitTime = waitTime;
        }
    }
}