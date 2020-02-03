using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Triangle
{
    public Vector3 VertexNumber1;
    public Vector3 VertexNumber2;
    public Vector3 VertexNumber3;

    public Triangle(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        if (point1 == point2 || point1 == point3 || point2 == point3)
        {
            Debug.LogError("You create Triangle with same vertexes. Check your algorithm");
        }
        VertexNumber1 = point1;
        VertexNumber2 = point2;
        VertexNumber3 = point3;
    }

    public bool ExistVertex(Vector3 vertex)
    {
        return VertexNumber1 == vertex || VertexNumber2 == vertex || VertexNumber3 == vertex ;
    }

    public bool OnOneSidePlane(Plane plane)
    {
        var side1 = plane.GetSide(VertexNumber1);
        var side2 = plane.GetSide(VertexNumber2);
        var side3 = plane.GetSide(VertexNumber3);
        return side1 == side2 && side1 == side3 ;
    }
}
