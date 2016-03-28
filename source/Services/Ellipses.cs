using System;
using System.Collections;
using UnityEngine;

namespace AnotherTerrain.Services
{
    public class DrawTriangle : MonoBehaviour
    {
        // Builds a mesh containing a single triangle with uv's.
        void Start()
        {
            MeshFilter mf = gameObject.AddComponent<MeshFilter>();
            MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh.Clear();
            mesh.vertices[0] = new Vector3(0, 0, 0);
            mesh.vertices[1] = new Vector3(0, 1, 0);
            mesh.vertices[2] = new Vector3(1, 1, 0);

            mesh.uv[0] = new Vector2(0, 0);
            mesh.uv[1] = new Vector2(0, 1);
            mesh.uv[2] = new Vector2(1, 1);
            mesh.triangles[0] = 0;
            mesh.triangles[1] = 1;
            mesh.triangles[2] = 2;
        }
    }

    public class EllipsesDraw : MonoBehaviour
    {
        float theta_scale = 0.01f;        //Set lower to add more points
        int size; //Total number of points in circle
        float radius = 3f;
        LineRenderer lineRenderer;

        void Awake()
        {
            float sizeValue = (2.0f * Mathf.PI) / theta_scale;
            size = (int)sizeValue;
            size++;
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.SetWidth(0.02f, 0.02f); //thickness of line
            lineRenderer.SetVertexCount(size);
        }

        void Update()
        {
            Vector3 pos;
            float theta = 0f;
            for (int i = 0; i < size; i++)
            {
                theta += (2.0f * Mathf.PI * theta_scale);
                float x = radius * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(theta);
                x += gameObject.transform.position.x;
                y += gameObject.transform.position.y;
                pos = new Vector3(x, y, 0);
                lineRenderer.SetPosition(i, pos);
            }
        }
    }

    public class Circle : MonoBehaviour
    {
        public int segments;

        public float xradius;
        public float yradius;
        LineRenderer line;

        void Start()
        {
            line = gameObject.GetComponent<LineRenderer>();
            line.SetVertexCount(segments + 1);
            line.useWorldSpace = false;
            CreatePoints();
        }

        void CreatePoints()
        {
            float x;
            float y;
            float z = 0f;
            float angle = 20f;

            for (int i = 0; i < (segments + 1); i++)
            {
                x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
                y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;
                line.SetPosition(i, new Vector3(x, y, z));
                angle += (360f / segments);
            }
        }
    }

    public class Star : MonoBehaviour
    {
        public Vector3[] points;
        public int frequency = 1;

        public Vector3 point = Vector3.up;
        public int numberOfPoints = 5;

        public Mesh mesh;

        private Vector3[] vertices;
        private int[] triangles;

        void Start()
        {
            UpdateMesh();
        }

        public void UpdateMesh()
        {
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "Star Mesh";

            if (frequency < 1)
            {
                frequency = 1;
            }
            if (points == null)
            {
                points = new Vector3[0];
            }
            int numberOfPoints = frequency * points.Length;
            vertices = new Vector3[numberOfPoints + 1];
            triangles = new int[numberOfPoints * 3];

            if (numberOfPoints >= 3)
            {
                float angle = -360f / numberOfPoints;
                for (int repetitions = 0, v = 1, t = 1; repetitions < frequency; repetitions++)
                {
                    for (int p = 0; p < points.Length; p += 1, v += 1, t += 3)
                    {
                        vertices[v] = Quaternion.Euler(0f, 0f, angle * (v - 1)) * points[p];
                        triangles[t] = v;
                        triangles[t + 1] = v + 1;
                    }
                }
                triangles[triangles.Length - 1] = 1;
            }
            mesh.vertices = vertices;
            mesh.triangles = triangles;
        }
    }
}
