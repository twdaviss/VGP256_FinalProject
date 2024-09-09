using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VerletChainSimulation : MonoBehaviour
{
    [SerializeField] private float gravityScale = 0.0f;
    [SerializeField] private float groundBounce = 0.5f;
    [SerializeField] private float drag = 0.9f;
    [SerializeField] private int numLinks;
    [SerializeField] private float linkRadius;

    private Circle[] links;

    void Start()
    {
        for (int i = 0; i < numLinks; i++)
        {
            links[i] = new Circle(0, 0, linkRadius);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < links.Length; i++)
        {
            float dx = (links[i].vertex.x - links[i].vertex.lx) * drag;  // get the speed and direction as a vector 
            float dy = (links[i].vertex.y - links[i].vertex.ly) * drag;  // including drag

            links[i].vertex.lx = links[i].vertex.x;   // set the last position to the current
            links[i].vertex.ly = links[i].vertex.y;
            links[i].vertex.x += dx;         // add the current movement
            links[i].vertex.y += dy;
            links[i].vertex.y += gravityScale;

            //links[i].CollisionDetection()
        }
    }
}
