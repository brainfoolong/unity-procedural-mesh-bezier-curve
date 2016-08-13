/*
Unity Procedural Mesh Bezier Curve Tutorial
Created by BrainFooLong @ July 2016
Youtube Series: https://www.youtube.com/playlist?list=PL8XNf8lax18m-RGjWivOpAqaFBqHDQ-X_
Github Repository: https://github.com/brainfoolong/unity-procedural-mesh-bezier-curve
License: MIT
*/

using UnityEngine;
using System.Collections;

public class BezierCurveData{

    /// <summary>
    /// BottomLeft, BottomRight, TopLeft, TopRight curve points
    /// </summary>
    public Vector3[] points;

    /// <summary>
    /// Get specific point on curve
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector3 GetPoint(float t)
    {
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        Vector3 point = uuu * points[0];
        point += 3 * uu * t * points[1];
        point += 3 * u * tt * points[2];
        point += ttt * points[3];

        return point;
    }

    /// <summary>
    /// Get approximate curve length
    /// </summary>
    /// <param name="granularity"></param>
    /// <returns></returns>
    public float GetApproximateLength(int granularity)
    {
        var length = 0f;
        var lastPoint = points[0];
        for (int i = 1; i <= granularity; i++)
        {
            var currentPoint = GetPoint(1f / granularity * i);
            length += Vector3.Distance(lastPoint, currentPoint);
            lastPoint = currentPoint;
        }
        return length;
    }	
}
