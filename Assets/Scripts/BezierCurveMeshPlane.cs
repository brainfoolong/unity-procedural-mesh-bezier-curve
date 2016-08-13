/*
Unity Procedural Mesh Bezier Curve Tutorial
Created by BrainFooLong @ July 2016
Youtube Series: https://www.youtube.com/playlist?list=PL8XNf8lax18m-RGjWivOpAqaFBqHDQ-X_
Github Repository: https://github.com/brainfoolong/unity-procedural-mesh-bezier-curve
License: MIT
*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BezierCurveMeshPlane : MonoBehaviour {

    /// <summary>
    /// The plane types
    /// </summary>
    public enum types { Front, Back, Top, Bottom, Left, Right};
    /// <summary>
    /// The plane type
    /// </summary>
    public types type;

    /// <summary>
    /// The parent curve mesh
    /// </summary>
    public BezierCurveMesh curveMesh;

    /// <summary>
    /// List of rectangles (4 vectors3) the represent quad row bounds 
    /// </summary>
    public List<Vector3[]> quadRows;

    /// <summary>
    /// The current vertices
    /// </summary>
    public List<Vector3> vertices;

    /// <summary>
    /// The current uv mapping
    /// </summary>
    public List<Vector2> uv;

    /// <summary>
    /// The current triangle vertex indexes
    /// </summary>
    public List<int> triangles;

    /// <summary>
    /// The vertex to indexes cache
    /// </summary>
    public Dictionary<Vector3, int> vertexToIndexes;

    /// <summary>
    /// Previous quad row vertices
    /// </summary>
    public List<Vector3> previousQuadRowVertices;

    /// <summary>
    /// Current quad row vertices
    /// </summary>
    List<Vector3> currentQuadRowVertices;

    /// <summary>
    /// Get array of all types
    /// </summary>
    /// <returns></returns>
    public static Array GetTypesArray()
    {
        return Enum.GetValues(typeof(BezierCurveMeshPlane.types));
    }

    /// <summary>
    /// Create a instance of itself with all required components
    /// </summary>
    /// <param name="type"></param>
    /// <param name="curveMesh"></param>
    /// <returns></returns>
    public static BezierCurveMeshPlane Instantiate(types type, BezierCurveMesh curveMesh)
    {

        var planeObject = new GameObject();
        planeObject.name = "Plane" + type;
        planeObject.transform.SetParent(curveMesh.transform);

        planeObject.AddComponent<MeshFilter>();
        planeObject.AddComponent<MeshRenderer>();
        planeObject.AddComponent<MeshCollider>();
        planeObject.AddComponent<Rigidbody>().isKinematic = true;

        var planeMesh = planeObject.AddComponent<BezierCurveMeshPlane>();
        planeMesh.type = type;
        planeMesh.curveMesh = curveMesh;

        return planeMesh;
    }

    /// <summary>
    /// Create the mesh
    /// </summary>
    public void CreateMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshRenderer = GetComponent<MeshRenderer>();
        var meshCollider = GetComponent<MeshCollider>();

        vertices = new List<Vector3>();
        uv = new List<Vector2>();
        triangles = new List<int>();
        vertexToIndexes = new Dictionary<Vector3, int>();

        for (int i = 0; i < quadRows.Count; i++)
        {
            currentQuadRowVertices = new List<Vector3>();
            AddQuadRow(i);
            previousQuadRowVertices = currentQuadRowVertices;
        }

        var mesh = new Mesh();
        meshFilter.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshRenderer.material = curveMesh.path.material;

        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// Reset the mesh data
    /// </summary>
    public void ResetMesh()
    {
        quadRows = new List<Vector3[]>();
        previousQuadRowVertices = new List<Vector3>();
    }

    /// <summary>
    /// Add a quad row
    /// </summary>
    /// <param name="rowIndex"></param>
    void AddQuadRow(int rowIndex)
    {
        var bounds = quadRows[rowIndex];
        var quads = curveMesh.path.quadsPerRow;
        for (int quadIndex = 0; quadIndex < quads; quadIndex++)
        {
            var addVertices = new Vector3[4];
            var addUv = new Vector2[4];

            var offsetLeft = 1f / quads * quadIndex;
            var offsetLeftNext = 1f / quads * (quadIndex + 1);
            var offsetTop = curveMesh.path.uvTilesPerCurve / quadRows.Count * rowIndex;
            var offsetTopNext = curveMesh.path.uvTilesPerCurve / quadRows.Count * (rowIndex + 1);

            var vertexIndexBounds = new int[4];
            var quadVertexStartIndex = currentQuadRowVertices.Count;

            addVertices[0] = Vector3.Lerp(bounds[0], bounds[1], offsetLeft);
            addVertices[1] = Vector3.Lerp(bounds[0], bounds[1], offsetLeftNext);
            addVertices[2] = Vector3.Lerp(bounds[2], bounds[3], offsetLeft);
            addVertices[3] = Vector3.Lerp(bounds[2], bounds[3], offsetLeftNext);

            if(previousQuadRowVertices.Count > 0)
            {
                addVertices[0] = previousQuadRowVertices[quadVertexStartIndex + 2];
                addVertices[1] = previousQuadRowVertices[quadVertexStartIndex + 3];
            }

            addUv[0] = new Vector2(offsetLeft, offsetTop);
            addUv[1] = new Vector2(offsetLeftNext, offsetTop);
            addUv[2] = new Vector2(offsetLeft, offsetTopNext);
            addUv[3] = new Vector2(offsetLeftNext, offsetTopNext);

            for (int i = 0; i < addVertices.Length; i++)
            {
                if (vertexToIndexes.ContainsKey(addVertices[i]))
                {
                    vertexIndexBounds[i] = vertexToIndexes[addVertices[i]];
                }
                else{
                    vertexToIndexes[addVertices[i]] = vertices.Count;
                    vertexIndexBounds[i] = vertices.Count;
                    vertices.Add(addVertices[i]);
                    uv.Add(addUv[i]);
                }
            }

            triangles.Add(vertexIndexBounds[0]);
            triangles.Add(vertexIndexBounds[2]);
            triangles.Add(vertexIndexBounds[1]);

            triangles.Add(vertexIndexBounds[2]);
            triangles.Add(vertexIndexBounds[3]);
            triangles.Add(vertexIndexBounds[1]);

            currentQuadRowVertices.AddRange(addVertices);
        }
    }
}
