/*
Unity Procedural Mesh Bezier Curve Tutorial
Created by BrainFooLong @ July 2016
Youtube Series: https://www.youtube.com/playlist?list=PL8XNf8lax18m-RGjWivOpAqaFBqHDQ-X_
Github Repository: https://github.com/brainfoolong/unity-procedural-mesh-bezier-curve
License: MIT
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BezierCurveMesh : MonoBehaviour {

    /// <summary>
    /// All curve point anchors
    /// </summary>
    public Transform[] anchors;

    /// <summary>
    /// The curve index
    /// </summary>
    public int curveIndex;

    /// <summary>
    /// The parent path
    /// </summary>
    public BezierCurvePath path;

    /// <summary>
    /// The path helper transform
    /// </summary>
    public Transform pathHelper;

    /// <summary>
    /// All planes in this mesh
    /// </summary>
    public Dictionary<BezierCurveMeshPlane.types, BezierCurveMeshPlane> planes;

    /// <summary>
    /// Create a instance of itself with all required components
    /// </summary>
    /// <param name="curveIndex"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static BezierCurveMesh Instantiate(int curveIndex, BezierCurvePath path)
    {
        GameObject tmpObject;
        var curveData = path.curveDatas[curveIndex];

        var curveObject = new GameObject();
        curveObject.name = "Curve" + curveIndex;
        curveObject.transform.SetParent(path.transform);
              
        var curveMesh = curveObject.AddComponent<BezierCurveMesh>();

        tmpObject = new GameObject();
        tmpObject.name = "PathHelper";
        curveMesh.pathHelper = tmpObject.transform;
        curveMesh.pathHelper.SetParent(curveObject.transform);

        curveMesh.anchors = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            tmpObject = new GameObject();
            tmpObject.name = "Anchor" + i;
            tmpObject.transform.SetParent(curveObject.transform);
            tmpObject.transform.localPosition = curveData.points[i];
            curveMesh.anchors[i] = tmpObject.transform;
        }

        curveMesh.planes = new Dictionary<BezierCurveMeshPlane.types, BezierCurveMeshPlane>();
        foreach(BezierCurveMeshPlane.types type in BezierCurveMeshPlane.GetTypesArray())
        {
            curveMesh.planes.Add(type, BezierCurveMeshPlane.Instantiate(type, curveMesh));
        }

        curveMesh.curveIndex = curveIndex;
        curveMesh.path = path;

        return curveMesh;
    }

    /// <summary>
    /// Create the mesh for all planes
    /// </summary>
    public void CreateMesh()
    {
        foreach(KeyValuePair<BezierCurveMeshPlane.types, BezierCurveMeshPlane> planeKV in planes)
        {
            planeKV.Value.ResetMesh();
            if(curveIndex > 0)
            {
                planeKV.Value.previousQuadRowVertices = path.curveMeshes[curveIndex - 1].planes[planeKV.Key].previousQuadRowVertices;
            }
        }

        var curveData = path.curveDatas[curveIndex];
        var curveTotalLength = curveData.GetApproximateLength((int)path.curveGranularity);
        var t = 0f;
        var tStep = 1f / path.curveGranularity;
        while ( t < 1f)
        {
            var curveLength = 0f;
            var tStart = t;
            while(curveLength < path.curveSegmentLength)
            {
                curveLength += Vector3.Distance(curveData.GetPoint(t), curveData.GetPoint(t + tStep));
                t += tStep;
            }

            if (t > 1f) t = 1f;

            var curvePointStart = curveData.GetPoint(tStart);
            var curvePointEnd = curveData.GetPoint(t);

            pathHelper.position = transform.position + curvePointStart;
            pathHelper.LookAt(transform.position + curvePointEnd);

            var right = pathHelper.right;
            var up = pathHelper.up;

            BezierCurveMeshPlane plane;

            if(curveIndex == 0)
            {
                plane = planes[BezierCurveMeshPlane.types.Front];
                if(plane.quadRows.Count == 0)
                {
                    plane.quadRows.Add(new Vector3[4] {
                        curvePointStart - up * path.cubeThickness,
                        curvePointStart + (right * path.cubeThickness) - (up * path.cubeThickness),
                        curvePointStart,
                        curvePointStart + right * path.cubeThickness
                    });
                }
            }

            if(curveIndex == path.curveDatas.Count - 1 && t == 1f)
            {
                plane = planes[BezierCurveMeshPlane.types.Back];
                if (plane.quadRows.Count == 0)
                {
                    plane.quadRows.Add(new Vector3[4] {
                        curvePointEnd + (right * path.cubeThickness) - (up * path.cubeThickness),
                        curvePointEnd - up * path.cubeThickness,
                        curvePointEnd + (right * path.cubeThickness),
                        curvePointEnd
                    });
                }
            }


            plane = planes[BezierCurveMeshPlane.types.Top];
            plane.quadRows.Add(new Vector3[4] {
                curvePointStart,
                curvePointStart + right * path.cubeThickness,
                curvePointEnd,
                curvePointEnd + right * path.cubeThickness                 
            });


            plane = planes[BezierCurveMeshPlane.types.Left];
            plane.quadRows.Add(new Vector3[4] {
                curvePointStart - up * path.cubeThickness,
                curvePointStart,
                curvePointEnd - up * path.cubeThickness,
                curvePointEnd
            });


            plane = planes[BezierCurveMeshPlane.types.Right];
            plane.quadRows.Add(new Vector3[4] {
                curvePointStart + right * path.cubeThickness,
                curvePointStart + (right * path.cubeThickness) - (up * path.cubeThickness),
                curvePointEnd + right * path.cubeThickness,
                curvePointEnd + (right * path.cubeThickness) - (up * path.cubeThickness),
            });


            plane = planes[BezierCurveMeshPlane.types.Bottom];
            plane.quadRows.Add(new Vector3[4] {
                curvePointStart + (right * path.cubeThickness) - (up * path.cubeThickness),
                curvePointStart - up * path.cubeThickness,
                curvePointEnd + (right * path.cubeThickness) - (up * path.cubeThickness),
                curvePointEnd - up * path.cubeThickness,
            });

        }

        foreach (KeyValuePair<BezierCurveMeshPlane.types, BezierCurveMeshPlane> planeKV in planes)
        {
            planeKV.Value.CreateMesh();
        }
    }    
}
