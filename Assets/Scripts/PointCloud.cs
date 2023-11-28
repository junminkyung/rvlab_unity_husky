//Author: Angel Ortiz
//Date: 08/15/17

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnitySensors;
using Unity.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloud : Sensor {

    private Mesh mesh;
    private VelodyneSensor lidarData;
    private int numPoints;
    private int[] indecies;
    private Color[] colors;
    private Vector3[] points;

    // Use this for initialization
    void Start() {
        mesh = new Mesh();
        lidarData = GameObject.Find("VelodyneSensor").GetComponent<VelodyneSensor>();
        lidarData.CompleteJob();
        // Debug.Log($"lidarData._pointsNum: {lidarData._pointsNum}");
        numPoints = lidarData._scanPattern.size;
        Debug.Log($"numPoints: {numPoints}");
        GetComponent<MeshFilter>().mesh = mesh;
        CreateMesh();
        
    }

    
    private void Update() {
        updateMesh();
    }

    void updateMesh() {
        lidarData.CompleteJob();
        points = lidarData.points.ToArray();
        mesh.vertices = points;
    }

    //Initializes mesh.
    void CreateMesh() {
        lidarData.CompleteJob();
        points = new Vector3[numPoints];
        indecies = new int[numPoints];
        colors = new Color[numPoints];
        points = lidarData.points.ToArray();
        for (int i = 0; i < points.Length; ++i) {
            indecies[i] = i;
            colors[i] = Color.white;
        }

        mesh.vertices = points;
        mesh.colors = colors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);

    }
}
