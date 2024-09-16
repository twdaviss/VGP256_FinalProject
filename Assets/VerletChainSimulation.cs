using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct Link
{
    public Link(float x, float y, float prevX, float prevY, float radius, float mass = 1.0f)
    {
        position.x = x;
        position.y = y;
        prevPosition.x = prevX;
        prevPosition.y = prevY;
        this.mass = mass;
        this.radius = radius;
    }
    public Link(Vector2 newPosition, Vector2 newPrevPosition, float radius, float mass = 1.0f)
    {
        position = newPosition;
        prevPosition = newPrevPosition;
        this.mass = mass;
        this.radius = radius;
    }

    public Vector2 position;
    public Vector2 prevPosition;
    public float mass;
    public float radius;
};

public class VerletChainSimulation : MonoBehaviour
{
    [Header("Node Parameters")]
    [SerializeField] private float gravityScale = 0.0f;
    [SerializeField] private float groundBounce = 0.5f;
    [SerializeField] private float drag = 0.9f;
    [SerializeField] private float linkRadius;
    [SerializeField] private float linkSeperation;
    [SerializeField] private int numLinks;
    [SerializeField] private int timeSteps;
    [SerializeField] private bool setLastNodeHeavy = false;
    [SerializeField] private GameObject ball;

    [Header("Link Elasticity")]
    [SerializeField] private int constraintIterations;

    private LineRenderer lineRenderer;

    private Link[] nodes;

    private bool isInTension = false;

    private float moveSpeed = 7.0f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numLinks;
        lineRenderer.startWidth = 2 * linkRadius;
        lineRenderer.endWidth = 2 * linkRadius;

        nodes = new Link[numLinks];
        for (int i = 0; i < numLinks; i++)
        {
            nodes[i] = new Link(1,-i * linkSeperation, 0, 0, linkRadius);
            lineRenderer.SetPosition(i, nodes[i].position);
        }
        if(setLastNodeHeavy)
        {
            nodes[numLinks - 1].mass = 10.0f;
            nodes[numLinks - 1].radius = linkRadius * 3;
        }
    }

    private void Update()
    {
        float adjustedMoveSpeed = moveSpeed;
        if (isInTension)
        {
            adjustedMoveSpeed *= 0.3f;
        }

        Vector2 moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        float newX = transform.position.x + (moveDirection.x * adjustedMoveSpeed * Time.deltaTime);
        float newY = transform.position.y + (moveDirection.y * adjustedMoveSpeed * Time.deltaTime);

        transform.position = new Vector3(newX, newY, 0);
    }

    private void FixedUpdate()
    {
        for (int step = 0; step < timeSteps; step++)
        {
            UpdatePositions(Time.fixedDeltaTime/ timeSteps);
            for (int i = 0; i < constraintIterations; i++)
            {
                CorrectNodeDistances();
                if(i % 2 == 0)
                {
                    ResolveCollisions();
                }
            }
        }
        UpdateVisuals();
    }

    private void UpdatePositions(float deltaTime)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            Link node = nodes[i];
            
            float dx = (node.position.x - node.prevPosition.x) * node.mass * drag * deltaTime;  // get the speed and direction as a vector 
            float dy = (node.position.y - node.prevPosition.y) * node.mass * drag * deltaTime;  // including drag

            nodes[i].prevPosition.x = node.position.x;  // set the last position to the current
            nodes[i].prevPosition.y = node.position.y;

            Vector2 newPos = new Vector2(nodes[i].position.x + dx, nodes[i].position.y + dy);

            Vector2 direction = newPos - nodes[i].position;
            float distance = direction.magnitude;
            direction.Normalize();

            int layerMask = LayerMask.GetMask("Obstacles");

            RaycastHit2D hit = Physics2D.CircleCast(nodes[i].position, nodes[i].radius, direction, distance, layerMask);
            if (hit)
            {
                newPos = hit.point + hit.normal * nodes[i].radius;
            }

            nodes[i].position = newPos;

            float gravity = gravityScale * nodes[i].mass * drag * deltaTime;
            nodes[i].position.y += gravity;
        }
    }

    private void CorrectNodeDistances()
    {
        nodes[0].position = transform.position;

        for (int i = 0; i < nodes.Length - 1; i++)
        {
            Link node1 = nodes[i];
            Link node2 = nodes[i+1];

            Vector2 dir1 = node1.position - node2.position;
            float dir2 = dir1.magnitude;
            float dir3 = (dir2 - linkSeperation) / dir2;

            Vector2 correction = (dir1 * (0.5f * dir3));

            nodes[i].position -= correction;
            nodes[i + 1].position += correction;
            if(i == nodes.Length - 2)
            {
                if(correction.magnitude > 0.0001f)
                {
                    isInTension = true;
                }
                else
                {
                    isInTension = false;
                }
            }
        }
    }

    private void ResolveCollisions()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            Vector2 position = new Vector2(nodes[i].position.x, nodes[i].position.y);
            int layerMask = LayerMask.GetMask("Obstacles");
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, nodes[i].radius, layerMask);

            foreach (Collider2D collider in colliders)
            {
                if (collider.isTrigger)
                {
                    continue;
                }
                Vector2 closestPoint = collider.ClosestPoint(position);
                float distance = Vector2.Distance(position, closestPoint);

                if (distance < nodes[i].radius)
                {
                    Vector2 overlapNormal = (position - closestPoint).normalized;
                    float depth = nodes[i].radius - distance;

                    Vector2 v = nodes[i].position - nodes[i].prevPosition;
                    Vector2 direction = 2 * (v * overlapNormal) * (overlapNormal - v).normalized;

                    Vector2 offset = overlapNormal * depth * 1.1f;
                    nodes[i].position += offset;

                    nodes[i].prevPosition = position - (direction * groundBounce * v.magnitude);
                }
            }
        }
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < nodes.Length; ++i)
        {
            lineRenderer.SetPosition(i, nodes[i].position);
        }
        if (setLastNodeHeavy)
        {
            float scale = nodes[numLinks - 1].radius * 12;
            ball.transform.localScale = new Vector3(scale, scale, scale);
            ball.transform.position = nodes[numLinks - 1].position;
        }
    }
}
