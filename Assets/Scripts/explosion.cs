using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.ExceptionServices;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using UnityEditor.MemoryProfiler;
using System.Linq;
using System.Threading;
using System.Transactions;


//note that i hate c# for its reference type lists which leads to unexpected things happening when changing a list.
public class explosion : MonoBehaviour
{

    public GameObject fragment;
    public GameObject frame;

    public int num;
    public float buffer = 0.1f;
    public bool delaunayMode = false;
    public bool triangulateOnStart = false;


    private List<List<Vector2>> realActualTriangles;
    private List<Vector2> pointsCopy;

    private int trianglesAdded = 0;

    private float xDimention;
    private float yDimention;
    private float zDimention;

    // Start is called before the first frame update
    void Start()
    {
        xDimention = transform.localScale.x;
        yDimention = transform.localScale.y;
        zDimention = transform.localScale.z;

        if (triangulateOnStart)
        {
            StepOne();
        }
    }

    private void OnCollisionEnter(Collision collider)
    {
        if (collider.gameObject.tag == "Activator")
        {
            if (triangulateOnStart)
            {
                ReplaceWall(realActualTriangles);
            }
            else
            {
                StepOne();
                ReplaceWall(realActualTriangles);
            }
        }
    }

    private void StepOne()
    {
        List<Vector2> points = new List<Vector2>();

        // generates the random points
        float randX;
        float randY;
        for (int i = 0; i < num; i++)
        {
            randX = Random.Range(-xDimention / 2 + buffer, xDimention / 2 - buffer);//so the points arnt on the frame itself
            randY = Random.Range(-yDimention / 2 + buffer, yDimention / 2 - buffer);

            points.Add(new Vector2(randX, randY));
        }

        //foreach (Vector2 point in points)
        //{
        //    addRecord(point.x, point.y, Application.dataPath + "/CSV/points.csv");//used for recording the randomly generated points for debugging
        //}

        //points.Clear();

        //points.Add(new Vector2(0.2117496f, -3.517347f));
        //points.Add(new Vector2(-1.07984f, 1.61447f));
        //points.Add(new Vector2(0.1885061f, 1.329544f));
        //points.Add(new Vector2(3.493232f, 4.150081f));
        //points.Add(new Vector2(1.459583f, 2.028693f));
        //points.Add(new Vector2(-0.4128866f, 2.125602f));
        //points.Add(new Vector2(-4.938151f, 0.142241f));


        //we need to make a copy of the points as the triagulate modifies it.
        pointsCopy = new List<Vector2>();

        foreach (Vector2 point in points)
        {
            pointsCopy.Add(point);
        }

        realActualTriangles = Triangulate(points);
    }

    private void ReplaceWall(List<List<Vector2>> triangles)
    {
        //make prisms from all of the triangles
        foreach (List<Vector2> triangle in triangles)
        {
            MakePrism(triangle);
        }


        //make the container for all the prisms made from the convex hull
        List<Vector2> convexHull = ConvexHull(pointsCopy);

        //the last item is a repeat of the first so we need to remove it
        convexHull.RemoveAt(convexHull.Count - 1);

        //makes the frame
        MakeFrame(convexHull);

        //gets rid of the original wall.
        Destroy(gameObject);
    }
    private void addRecord(float x, float y, string filepath)
    {
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filepath, true))
        {
            file.WriteLine(Convert.ToString(x) + "," + Convert.ToString(y));
        }
    }

    private void MakeFrame(List<Vector2> hullPoints)
    {
        int hullNum = hullPoints.Count;

        List<Vector2> corners = new List<Vector2>()
        {
            new Vector2(-xDimention/2, yDimention/2),
            new Vector2(xDimention/2, yDimention/2),
            new Vector2(xDimention/2, -yDimention/2),
            new Vector2(-xDimention/2, -yDimention/2)
        };

        Vector3[] vertices = new Vector3[(hullNum + 4) * 2];//the hull points plus the four corners all *2 for the extruded side.

        //makes the vertices in a specific format -- the four front corners, the front hull points clockwise from the toppoint, the back corners, the back huller points same order. 
        for (int i = 0; i < (hullPoints.Count + 4) * 2; i++)
        {
            if (i < 4)
            {
                vertices[i] = corners[i];
            }
            else if (i < hullNum + 4)
            {
                vertices[i] = hullPoints[i - 4];
            }
            else if (i < hullNum + 8)
            {
                vertices[i] = new Vector3(corners[i - hullNum - 4].x, corners[i - hullNum - 4].y, zDimention);
            }
            else
            {
                vertices[i] = new Vector3(hullPoints[i - hullNum - 8].x, hullPoints[i - hullNum - 8].y, zDimention);
            }
        }

        int[] frontTriangles = new int[(hullPoints.Count + 4) * 3];


        List<Vector2> firstVisible = GetVisible(corners[0], hullPoints);
        List<Vector2> secondVisible = GetVisible(corners[1], hullPoints);
        List<Vector2> thirdVisible = GetVisible(corners[2], hullPoints);
        List<Vector2> fourthVisible = GetVisible(corners[3], hullPoints);

        //adds the all the triangles with an edge on the convex hull
        int firstBackOverlap = FindBackOverlap(fourthVisible, firstVisible);
        frontTriangles = AddTriangles(frontTriangles, corners[0], hullPoints, firstBackOverlap, 0);

        int secondBackOverlap = FindBackOverlap(firstVisible, secondVisible);
        frontTriangles = AddTriangles(frontTriangles, corners[1], hullPoints, secondBackOverlap, 1);

        int thirdBackOverlap = FindBackOverlap(secondVisible, thirdVisible);
        frontTriangles = AddTriangles(frontTriangles, corners[2], hullPoints, thirdBackOverlap, 2);

        int fourthBackOverlap = FindBackOverlap(thirdVisible, fourthVisible);
        frontTriangles = AddTriangles(frontTriangles, corners[3], hullPoints, fourthBackOverlap, 3);

        //adds the four triangles with an edge on the frame square currently
        frontTriangles[trianglesAdded * 3] = 0;
        frontTriangles[trianglesAdded * 3 + 2] = hullPoints.IndexOf(firstVisible[firstVisible.Count - 1]) + 4;
        frontTriangles[trianglesAdded * 3 + 1] = 1;

        trianglesAdded++;

        frontTriangles[trianglesAdded * 3] = 1;
        frontTriangles[trianglesAdded * 3 + 2] = hullPoints.IndexOf(secondVisible[secondVisible.Count - 1]) + 4;
        frontTriangles[trianglesAdded * 3 + 1] = 2;

        trianglesAdded++;

        frontTriangles[trianglesAdded * 3] = 2;
        frontTriangles[trianglesAdded * 3 + 2] = hullPoints.IndexOf(thirdVisible[thirdVisible.Count - 1]) + 4;
        frontTriangles[trianglesAdded * 3 + 1] = 3;

        trianglesAdded++;

        frontTriangles[trianglesAdded * 3] = 3;
        frontTriangles[trianglesAdded * 3 + 2] = hullPoints.IndexOf(fourthVisible[fourthVisible.Count - 1]) + 4;
        frontTriangles[trianglesAdded * 3 + 1] = 0;

        trianglesAdded++;

        int trans = hullNum + 4; // the amount to go from a front verticy to a back one

        int[] backTriangles = new int[frontTriangles.Length];

        for (int i = 0; i < backTriangles.Length; i++)
        {
            if (i % 3 == 1)//switches the order of the tringles from clockwise to anticlockwise
            {
                backTriangles[i + 1] = frontTriangles[i] + trans;//translates the front triangles to th evack as well
            }
            else if (i % 3 == 2)
            {
                backTriangles[i - 1] = frontTriangles[i] + trans;
            }
            else
            {
                backTriangles[i] = frontTriangles[i] + trans;
            }
        }

        int[] insideTriangles = new int[(hullNum * 2) * 3];
        int insideTrianglesAdded = 0;

        for (int i = 0; i < hullNum; i++)
        {
            if (i == hullNum - 1)
            {
                insideTriangles[insideTrianglesAdded * 3] = i + 4;
                insideTriangles[insideTrianglesAdded * 3 + 1] = 4;
                insideTriangles[insideTrianglesAdded * 3 + 2] = i + 4 + trans;

                insideTrianglesAdded++;

                insideTriangles[insideTrianglesAdded * 3] = i + 4 + trans;
                insideTriangles[insideTrianglesAdded * 3 + 1] = 4;
                insideTriangles[insideTrianglesAdded * 3 + 2] = 4 + trans;
            }
            else
            {

                insideTriangles[insideTrianglesAdded * 3] = i + 4;
                insideTriangles[insideTrianglesAdded * 3 + 1] = i + 4 + 1;
                insideTriangles[insideTrianglesAdded * 3 + 2] = i + 4 + trans;

                insideTrianglesAdded++;

                insideTriangles[insideTrianglesAdded * 3] = i + 4 + trans;
                insideTriangles[insideTrianglesAdded * 3 + 1] = i + 4 + 1;
                insideTriangles[insideTrianglesAdded * 3 + 2] = i + 4 + 1 + trans;

                insideTrianglesAdded++;
            }
        }

        int[] sideTriangles = new int[24]
        {
            0,
            3,
            3 + trans,
            0 + trans,
            0,
            3 + trans,
            0,
            1 + trans,
            1,
            0,
            0 + trans,
            1 + trans,
            1,
            2 + trans,
            2,
            1,
            1 + trans,
            2 + trans,
            3,
            2,
            2 + trans,
            3,
            2 + trans,
            3 + trans
    };


        int[] allTriangles = new int[(8 + (hullNum * 2) + ((hullNum + 4) * 2)) * 3];

        allTriangles = Concatenate(Concatenate(frontTriangles, insideTriangles), (Concatenate(backTriangles, sideTriangles)));

        //create a new mesh object
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;

        mesh.triangles = allTriangles;

        mesh.RecalculateNormals();

        //create the object with the same position as the wall as the mesh coords handle the actual position in the wall
        GameObject newFragment = Instantiate(frame, gameObject.GetComponent<Transform>().position, gameObject.GetComponent<Transform>().rotation);
        newFragment.GetComponent<MeshFilter>().mesh = mesh;
        newFragment.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private T[] Concatenate<T>(T[] first, T[] second)
    {
        if (first == null)
        {
            return second;
        }
        if (second == null)
        {
            return first;
        }

        return first.Concat(second).ToArray();
    }
    private int[] AddTriangles(int[] triangles, Vector2 source, List<Vector2> hullPoints, int overlapFromLastOfPreviousNode, int sourceNodePos)
    {
        List<Vector2> visablePoints = GetVisible(source, hullPoints);

        bool loop = true;
        int currIndex = overlapFromLastOfPreviousNode;

        if (currIndex != visablePoints.Count - 1) // to exclude the cases when a corner does not actually have any triangles
        {
            while (loop)
            {
                triangles[trianglesAdded * 3] = sourceNodePos;
                triangles[trianglesAdded * 3 + 2] = hullPoints.IndexOf(visablePoints[currIndex]) + 4;
                triangles[trianglesAdded * 3 + 1] = hullPoints.IndexOf(visablePoints[currIndex + 1]) + 4;

                currIndex++;

                trianglesAdded += 1;

                if (currIndex == visablePoints.Count - 1)
                {
                    loop = false;
                }
            }
        }
        return triangles;
    }

    private int FindBackOverlap(List<Vector2> lastNodeVisable, List<Vector2> currentNodeVisable)
    {
        return (currentNodeVisable.IndexOf(lastNodeVisable[lastNodeVisable.Count - 1]));
    }

    private List<Vector2> GetVisible(Vector2 source, List<Vector2> hull)
    {
        List<Vector2> visible = new List<Vector2>();

        List<float> angles = new List<float>();

        foreach (Vector2 point in hull)
        {
            angles.Add(Vector2.SignedAngle(Vector2.up, point - source));
        }

        int maxIndex = FindMaxIndex(angles);
        int minIndex = FindMinIndex(angles);

        bool loop = true;

        int curr = minIndex;

        while (loop)
        {
            if (curr == maxIndex)
            {
                loop = false;
            }

            visible.Add(hull[curr]);

            if (curr == hull.Count - 1)
            {
                curr = 0;
            }
            else
            {
                curr++;
            }
        }

        return visible;

    }
    private int FindMaxIndex(List<float> angles)
    {
        int curr = 0;

        for (int i = 1; i < angles.Count; i++)
        {
            if (angles[i] > angles[curr])
            {
                curr = i;
            }
        }
        return curr;
    }

    private int FindMinIndex(List<float> angles)
    {
        int curr = 0;

        for (int i = 1; i < angles.Count; i++)
        {
            if (angles[i] < angles[curr])
            {
                curr = i;
            }
        }
        return curr;
    }

    private List<List<Vector2>> TriagulationInitiation(List<Vector2> hull, Vector2 firstPoint)//TODO - add delaunay mode
    {
        List<List<Vector2>> tris = new List<List<Vector2>>();


        for (int i = 0; i < hull.Count - 1; i++)
        {
            tris.Add(new List<Vector2>() {
                firstPoint,
                hull[i],
                hull[i + 1]
            });
        }

        return tris;
    }


    private List<List<Vector2>> ConvexHullOnly(List<Vector2> hull)//draw lines from the first point of the hull to every other node for the triangulation
    {
        List<List<Vector2>> tris = new List<List<Vector2>>();//TODO - add delaunay

        Vector2 sourceNode = hull[0];

        //the last thing in the hull is still the first aswell so we will need to remove it for this to work
        hull.RemoveAt(hull.Count - 1);

        for (int i = 1; i < hull.Count - 1; i++)
        {
            tris.Add(new List<Vector2>()
            {
                sourceNode,
                hull[i],
                hull[i+1]
            });
        }

        return tris;
    }
    private List<List<Vector2>> Triangulate(List<Vector2> points)
    {
        List<Vector2> convexHull = ConvexHull(points);

        // the last element is also the top node so needs removing however we will use it in the first triangulation ininitialisation so it needs to stay for that to work
        //convexHull.RemoveAt(convexHull.Count - 1);


        //removes the points in the conves hull from the points list
        foreach (Vector2 xtreme in convexHull)
        {
            points.Remove(xtreme);
        }

        //copy array as arrays are reference types in c#
        List<Vector2> addedPoints = new List<Vector2>();

        //if there is at least one pointd inside the convex hull
        List<List<Vector2>> triangles;
        if (points.Count != 0)
        {
            triangles = TriagulationInitiation(convexHull, points[0]);

            //move the first points from points to added points
            addedPoints.Add(points[0]);
            points.RemoveAt(0);

        }
        else //if there aren't any points inside the convex hull
        {
            triangles = ConvexHullOnly(convexHull);
        }

        foreach (Vector2 coord in convexHull)
        {
            addedPoints.Add(coord);
        }

        addedPoints.RemoveAt(addedPoints.Count - 1); //because the first and last are duplicates so i remove one which reduces number of checks later

        //initialisation now done its time to loop through all the remaining points and add them to the network
        foreach (Vector2 point in points)
        {
            List<List<Vector2>> rays = new List<List<Vector2>>();

            //generates the list of rays
            foreach (Vector2 node in addedPoints)
            {
                rays.Add(new List<Vector2>() { point, node });
            }

            List<List<Vector2>> allLines = new List<List<Vector2>>();

            //generates the list of all line in the current network
            foreach (List<Vector2> triangle in triangles)
            {
                allLines.Add(new List<Vector2>()
                {
                    triangle[0],
                    triangle[1]
                });

                allLines.Add(new List<Vector2>()
                {
                    triangle[1],
                    triangle[2]
                });

                allLines.Add(new List<Vector2>()
                {
                    triangle[2],
                    triangle[0]
                });
            }

            List<List<Vector2>> newLines = new List<List<Vector2>>();

            foreach (List<Vector2> ray in rays)
            {
                bool hasCrossed = false;

                foreach (List<Vector2> line in allLines)
                {
                    if (ProperlyIntersect(ray[0], ray[1], line[0], line[1]))
                    {
                        hasCrossed = true;
                    }
                }

                if (!hasCrossed)
                {
                    newLines.Add(ray);
                }
            }

            if (newLines.Count < 3)
            {
                print("there is a error with the properIntersection algorithm from rounding errors");
            }

            //adds the three new triangles to the triangles list found by the three non intercepting rays 
            triangles.Add(new List<Vector2>() { point, newLines[0][1], newLines[1][1] });

            triangles.Add(new List<Vector2>() { point, newLines[1][1], newLines[2][1] });

            triangles.Add(new List<Vector2>() { point, newLines[2][1], newLines[0][1] });

            //removes the large triangle the new point was added into as it is now three smaller triangles
            //there are six combination for the order of the nodes as 3 * 2 * 1 = 6 so you have to try to remove all six combinations
            //the first item in newLines is the new point so we want the second item which is a node of the big triangle

            for (int i = 0; i < triangles.Count; i++)
            {
                if ((triangles[i].Contains(newLines[0][1])) && (triangles[i].Contains(newLines[1][1])) && (triangles[i].Contains(newLines[2][1])))
                {
                    triangles.RemoveAt(i);
                }
            }

            //Re-Delauniate if that option is selected
            if (delaunayMode)
            {
                triangles = reDelauniate(triangles);
            }

            //finally add the point to the added points for the future points' checking needs
            addedPoints.Add(point);
        }

        return triangles;
    }

    private List<List<Vector2>> reDelauniate(List<List<Vector2>> tris)//assumes that before the point was added the triangulation was delaunay
    {

        //the last three triangles in the array are the new triangles

        tris = RecursiveFix(tris.Count - 1, tris);
        tris = RecursiveFix(tris.Count - 2, tris);
        tris = RecursiveFix(tris.Count - 3, tris);

        return tris;
    }

    private List<List<Vector2>> RecursiveFix(int triIndex, List<List<Vector2>> tris)
    {

        List<int> needChecking = new List<int>()
        {
            triIndex
        };

        while (needChecking.Count != 0)
        {

            int currTriangleIndex = needChecking[0];
            int currTrianglePairIndex = FindPairIndex(currTriangleIndex, 1, 2, tris);//1 and 2 as they are always the points that are not the newly added point

            if (currTrianglePairIndex != -1)//if there is a pairing triangle
            {
                if (FlipTest(currTriangleIndex, currTrianglePairIndex, tris))
                {
                    tris = Flip(currTriangleIndex, currTrianglePairIndex, tris);
                    needChecking.Add(currTrianglePairIndex);//now the two triangles are two new triangles that both need checking so we add the paired triangle to the list as the currTriangle is still on the list
                }
                else
                {
                    needChecking.RemoveAt(0);
                }
            }
            else
            {
                needChecking.RemoveAt(0);
            }
        }
        return tris;
    }

    private int FindPairIndex(int triIndex, int startEdgeIndex, int endEdgeIndex, List<List<Vector2>> tris)//returns the index of the triangle that pairs that given on the given edge if such exists (-1 if not)
    {
        List<Vector2> currentTriangle = tris[triIndex];
        foreach (List<Vector2> triangle in tris)
        {
            if ((triangle.Contains(currentTriangle[startEdgeIndex]) && triangle.Contains(currentTriangle[endEdgeIndex])) && tris.IndexOf(triangle) != triIndex)
            {
                return tris.IndexOf(triangle);
            }
        }

        return -1;
    }

    private bool FlipTest(int tri1Index, int tri2Index, List<List<Vector2>> tris)
    {
        List<Vector2> edgePair = GetOrderedNodes(tris[tri1Index], tris[tri2Index]);

        float totalAngle = Vector2.Angle(edgePair[2] - edgePair[0], edgePair[3] - edgePair[0]) + Vector2.Angle(edgePair[2] - edgePair[1], edgePair[3] - edgePair[1]);

        bool flip = false;

        if (totalAngle > 180)
        {
            flip = true;
        }

        return flip;
    }

    private List<List<Vector2>> Flip(int tri1Index, int tri2Index, List<List<Vector2>> tris)//its important that the first node in the new triangles is always the point not on the original shared edge of the first triangle
    {
        List<Vector2> edgePair = GetOrderedNodes(tris[tri1Index], tris[tri2Index]);

        tris[tri1Index] = new List<Vector2>()
            {
                edgePair[0],
                edgePair[1],
                edgePair[2]
            };
        tris[tri2Index] = new List<Vector2>()
            {
                edgePair[0],
                edgePair[1],
                edgePair[3]
            };

        return tris;
    }


    private List<Vector2> GetOrderedNodes(List<Vector2> tri1, List<Vector2> tri2)//The first two nodes returned are those not on the shared edge the second two are(the first is the node in tri1 not on the shared edge)
    {
        List<Vector2> output = new List<Vector2>();

        for (int i = 0; i < 3; i++)
        {
            if (tri1.Contains(tri2[i]))
            {
                output.Add(tri2[i]);
            } else
            {
                output.Insert(0, tri2[i]);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (!(tri2.Contains(tri1[i])))
            {
                output.Insert(0, tri1[i]);
            }
        }
        return output;
    }




    private List<decimal> GetIntercept(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
    {
        decimal decPoint1x = Convert.ToDecimal(point1.x);
        decimal decPoint1y = Convert.ToDecimal(point1.y);

        decimal decPoint2x = Convert.ToDecimal(point2.x);
        decimal decPoint2y = Convert.ToDecimal(point2.y);

        decimal decPoint3x = Convert.ToDecimal(point3.x);
        decimal decPoint3y = Convert.ToDecimal(point3.y);

        decimal decPoint4x = Convert.ToDecimal(point4.x);
        decimal decPoint4y = Convert.ToDecimal(point4.y);

        decimal grad1 = (decPoint1y - decPoint2y) / (decPoint1x - decPoint2x);
        decimal grad2 = (decPoint3y - decPoint4y) / (decPoint3x - decPoint4x);

        decimal yIntercept1 = (decPoint1y) - ((grad1) * (decPoint1x));
        decimal yIntercept2 = (decPoint3y) - ((grad2) * (decPoint3x));

        decimal x = (yIntercept2 - yIntercept1) / (grad1 - grad2);
        decimal y = (grad1) * (x) + (yIntercept1);

        return new List<decimal>()
        {
            x,
            y
        };
    }

    private bool ProperlyIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
    {
        //check for a proper intersection if so then makes hasCrossed true
        List<decimal> interceptionPoint = GetIntercept(point1, point2, point3, point4);

        float maxLine1;
        float minLine1;
        float maxLine2;
        float minLine2;

        bool firstCheck = false;
        bool secondCheck = false;
        bool thirdCheck = false;
        bool fourthCheck = false;

        if (point1.x > point2.x)
        {
            maxLine1 = point1.x;
            minLine1 = point2.x;
        }
        else
        {
            maxLine1 = point2.x;
            minLine1 = point1.x;
        }

        if (point3.x > point4.x)
        {
            maxLine2 = point3.x;
            minLine2 = point4.x;
        }
        else
        {
            maxLine2 = point4.x;
            minLine2 = point3.x;
        }

        float xCoord = Convert.ToSingle(interceptionPoint[0]);

        firstCheck = xCoord > minLine1 + 0.00001;
        secondCheck = xCoord < maxLine1 - 0.00001;

        thirdCheck = xCoord > minLine2 + 0.00001;
        fourthCheck = xCoord < maxLine2 - 0.00001;


        if (firstCheck && secondCheck && thirdCheck && fourthCheck)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void MakePrism(List<Vector2> points)
    {

        //vector3 and vector2 can be implicitly converted between oneanother
        Vector3[] vertices = new Vector3[6];

        List<Vector2> frontTriangle = GetClockwise(points[0], points[1], points[2]);

        //adds the six vertices of the triangular prism in the clockwise order
        vertices[0] = frontTriangle[0];
        vertices[1] = frontTriangle[1];
        vertices[2] = frontTriangle[2];

        //the images of the points of the 2d triangle extrudes to 3d
        vertices[3] = new Vector3(vertices[0].x, vertices[0].y, zDimention);
        vertices[4] = new Vector3(vertices[1].x, vertices[1].y, zDimention);
        vertices[5] = new Vector3(vertices[2].x, vertices[2].y, zDimention);

        int[] triangles = new int[24];

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        //the back triangle - the order is changed because we want it to be counterclockwise because it is the back face for the backface culling
        triangles[3] = 3;
        triangles[4] = 5;
        triangles[5] = 4;

        //IMPORTANT to save much calculations, the subtriangle of the three long faces of a triangular prism will always be clockwise from outside
        //IFFFFF you read it from the shared 90degree corner (theyre always righangular) to the non face triangle sharing point to the face triangle sharing point
        //ALSO MEGA IMPORTANT - this order only works if the vertices 0,1,2 are in a clockwise order

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

        //create a new mesh object
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        //create the object with the same position as the wall as the mesh coords handle the actual position in the wall
        GameObject newFragment = Instantiate(fragment, gameObject.GetComponent<Transform>().position, gameObject.GetComponent<Transform>().rotation);
        newFragment.GetComponent<MeshFilter>().mesh = mesh;
        newFragment.GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    private List<Vector2> ConvexHull(List<Vector2> points)
    {
        //note that this function rerurns an list where the first and last elements are the same

        List<Vector2> convexHull = new List<Vector2>();

        float currHeight = -1000f;

        Vector2 topPoint = new Vector2();
        //find the top point
        foreach (Vector2 point in points)
        {
            if (point.y > currHeight)
            {
                topPoint = point;
                currHeight = point.y;
            }
            else
            {
            }
        }
        convexHull.Add(topPoint);

        float currAngle = -180;
        Vector2 currPoint = new Vector2();

        //find the first point on the convex hull after the top point
        foreach (Vector2 endPoint in points)
        {
            if (endPoint != topPoint)
            {

                float angle = Vector2.SignedAngle(Vector2.left, endPoint - topPoint);

                if (angle > currAngle)
                {
                    currPoint = endPoint;
                    currAngle = angle;
                }
            }
        }

        convexHull.Add(currPoint);

        //finds every point on the convext hull after that second point
        while (currPoint != topPoint)
        {
            currAngle = -180;

            foreach (Vector2 endPoint in points)
            {
                if ((endPoint != convexHull[convexHull.Count - 1]) && (endPoint != convexHull[convexHull.Count - 2]))
                {

                    Vector2 first = convexHull[convexHull.Count - 1] - convexHull[convexHull.Count - 2];
                    Vector2 second = endPoint - convexHull[convexHull.Count - 1];


                    float angle = Vector2.SignedAngle(first, second);

                    if (angle > currAngle)
                    {
                        currPoint = endPoint;
                        currAngle = angle;
                    }
                }
            }

            convexHull.Add(currPoint);
        }

        return convexHull;

    }

    private List<Vector2> GetClockwise(Vector2 point1, Vector2 point2, Vector2 point3) //can be simplified using signed angle function
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
        float angle1 = Vector2.SignedAngle(Vector2.up, otherPoint1 - topPoint);
        float angle2 = Vector2.SignedAngle(Vector2.up, otherPoint2 - topPoint);

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