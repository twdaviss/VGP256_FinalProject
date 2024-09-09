using UnityEngine;

public class Rectangle : MonoBehaviour
{
    [HideInInspector] public float width;
    [HideInInspector] public float height;
    [HideInInspector] public float minX;
    [HideInInspector] public float maxX;
    [HideInInspector] public float minY;
    [HideInInspector] public float maxY;
    [HideInInspector] private Vector2 centrePoint;

    void Start()
    {
        centrePoint = transform.position;
        width = transform.localScale.x;
        height = transform.localScale.y;
        minX = centrePoint.x - width/2;
        maxX = centrePoint.x + width/2;
        minY = centrePoint.y - height/2;
        maxY = centrePoint.y + height/2;
    }
}
