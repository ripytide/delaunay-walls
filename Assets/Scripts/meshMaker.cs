using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshFilter))]
public class meshMaker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MakePrism(new List<Vector2>()
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(0.5f,0.5f)
        }, 1f);
    }

    void MakePrism(List<Vector2> points, float depth)
    {
        //vector3 and vector2 can be implicitly converted between oneanother
        Vector3[] vertices = new Vector3[6];
        //adds the six vertices of the triangular prism
        vertices[0] = points[0];
        vertices[1] = points[1];
        vertices[2] = points[2];

        //the images of the points of the 2d triangle extrudes to 3d
        vertices[3] = new Vector3(points[0].x, points[0].y, depth);
        vertices[4] = new Vector3(points[1].x, points[1].y, depth);
        vertices[5] = new Vector3(points[2].x, points[2].y, depth);

        int[] triangles = new int[24];

        //the front triangle
        List<Vector2> frontTriangle = GetClockwise(vertices[0], vertices[1], vertices[2]);

        triangles[0] = Array.IndexOf(vertices, frontTriangle[0]);
        triangles[1] = Array.IndexOf(vertices, frontTriangle[1]);
        triangles[2] = Array.IndexOf(vertices, frontTriangle[2]);

        //the back triangle - the order is changed because we want it to be counterclockwise because it is the back face for the backface culling
        //+3 because GetClockwise returns the 2d vectors and we want the back versions which are 3 after the orinals so +3
        triangles[3] = Array.IndexOf(vertices, frontTriangle[0]) + 3;
        triangles[4] = Array.IndexOf(vertices, frontTriangle[2]) + 3;
        triangles[5] = Array.IndexOf(vertices, frontTriangle[1]) + 3;

        //IMPORTANT to save much calculations, the subtriangle of the three long faces of a triangular prism will always be clockwise from outside
        //IFFFFF you read it from the shared 90degree corner (theyre always righangular) to the non face triangle sharing point to the face triangle sharing point

        //the front triangle pair of xy plane sharing points
        triangles[6] = 0;
        triangles[7] = 3;
        triangles[8] = 1;

        triangles[9] = 1;
        triangles[10] = 4;
        triangles[11] = 2;

        triangles[12] = 2;
        triangles[13] = 5;
        triangles[14] = 0;

        //the back triangle pair of xy plan sharing points
        triangles[15] = 4;
        triangles[16] = 1;
        triangles[17] = 3;

        triangles[18] = 5;
        triangles[19] = 2;
        triangles[20] = 4;

        triangles[21] = 3;
        triangles[22] = 0;
        triangles[23] = 5;
    }

    private List<Vector2> GetClockwise(Vector2 point1, Vector2 point2, Vector2 point3)
    {
        Vector2 otherPoint1;
        Vector2 otherPoint2;
        Vector2 topPoint;

        if (point1.y > point2.y && point1.y > point3.y)
        {
            topPoint = point1;
            otherPoint1 = point2;
            otherPoint2 = point3;
        }
        else if (point2.y > point1.y && point2.y > point3.y)
        {
            topPoint = point2;
            otherPoint1 = point1;
            otherPoint2 = point3;
        }
        else
        {
            topPoint = point3;
            otherPoint1 = point1;
            otherPoint2 = point2;
        }

        //sighned angle is positive if its anticlockwise and negitive if clockwise so its -180 to 180  180 if opousite
        float angle1 = Vector2.SignedAngle(Vector2.up, otherPoint1);
        float angle2 = Vector2.SignedAngle(Vector2.up, otherPoint2);

        //there are three triangle scenarios: one with two points on the left of the top point, the two point on the right of the top point or one either side

        List<Vector2> clockwiseOrder;

        if ((otherPoint1.x > topPoint.x && otherPoint2.x > topPoint.x) || (otherPoint1.x < topPoint.x && otherPoint2.x < topPoint.x))
        {
            if (angle1 > angle2)
            {
                clockwiseOrder = new List<Vector2>()
                {
                    topPoint,
                    otherPoint1,
                    otherPoint2
                };
            }
            else
            {
                clockwiseOrder = new List<Vector2>()
                {
                    topPoint,
                    otherPoint2,
                    otherPoint1
                };
            }
        }
        else
        {
            if (angle1 > angle2)
            {
                clockwiseOrder = new List<Vector2>()
                {
                    topPoint,
                    otherPoint2,
                    otherPoint1
                };
            }
            else
            {
                clockwiseOrder = new List<Vector2>()
                {
                    topPoint,
                    otherPoint1,
                    otherPoint2
                };
            }
        }

        return clockwiseOrder;
    }

}
