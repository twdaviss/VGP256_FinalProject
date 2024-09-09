using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct Vertex
{
    public Vertex(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.lx = x;
        this.ly = y;
    }
    public float x;
    public float y;
    public float lx; //prev x position
    public float ly; //prev y position
};
public class Circle
{
    public Vertex vertex;
    private float radius;
    public Circle(float x, float y, float radius)
    {
        vertex = new Vertex(x, y);
        this.radius = radius;
    }
    public void CollisionDetection(GameObject collider, Vector2 position)
    {
        if(collider.GetComponent<Circle>() == null)
        {
            GameObject circle = collider.gameObject;
            float distance = Vector2.Distance(position, circle.transform.position);
            if (distance < circle.GetComponent<Circle>().radius + radius) // intersecting
            {
                Vector2 direction = (position - (Vector2)circle.transform.position).normalized;

                position = position + ((direction * (radius * 2 - distance)) / 2);
                circle.transform.position = (Vector2)circle.transform.position - ((direction * (radius * 2 - distance)) / 2);

            }
        }
        else if (collider.GetComponent<Rectangle>() != null)
        {
            Rectangle rectangle = collider.GetComponent<Rectangle>();

            if (Vector2.Distance(position, collider.transform.position) > 2 * Mathf.Max(rectangle.width, rectangle.height)) { return; }

            Vector2 edge = Vector2.zero;
            if (rectangle.minX < position.x && position.x < rectangle.maxX)
            {
                edge.x = position.x;
            }
            else if (position.x > rectangle.maxX)
            {
                edge.x = rectangle.maxX;
            }
            else if (position.x < rectangle.minX)
            {
                edge.x = rectangle.minX;
            }
            if (rectangle.minY < position.y && position.y < rectangle.maxY)
            {
                edge.y = position.y;
            }
            else if (position.y > rectangle.maxY)
            {
                edge.y = rectangle.maxY;
            }
            else if (position.y < rectangle.minY)
            {
                edge.y = rectangle.minY;
            }
            float distX = position.x - edge.x;
            float distY = position.y - edge.y;

            float distance = Mathf.Sqrt((distX * distX) + (distY * distY)); // distance from closest edge
            if (distance < radius) // intersecting
            {
                Vector2 direction = (Vector2)position - edge;
                float difference = radius - distance;
                Vector2 offset = direction.normalized * difference;
                position = position + offset; // moved back by the amount of interection in the opposite direction of the edge
            }
        }
    }
}
