using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Triangle = System.Tuple<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3>;

[RequireComponent(typeof(MeshFilter))]
public class ArbitraryMeshEncloseGrains : MonoBehaviour
{
    public int numGrains = 100;
    public GameObject[] grainObjs;

    List<float> probabilities = new();
    List<Triangle> triangles = new();
    Mesh mesh;

    private static Vector3 GetRandomPointInTriagnle(Triangle triangle)
    {
        float r1 = Random.Range(0.0f, 1.0f);
        float r2 = Random.Range(0.0f, 1.0f);

        Vector3 A = triangle.Item1;
        Vector3 B = triangle.Item2;
        Vector3 C = triangle.Item3;

        Vector3 p =
            (A * (1.0f - Mathf.Sqrt(r1))) +
            (B * (Mathf.Sqrt(r1) * (1.0f - r2))) +
            (C * (r2 * Mathf.Sqrt(r1)));

        return p;
    }

    private Triangle GetTriangle(int i, bool worldSpace)
    {
        Vector3 a = mesh.vertices[mesh.triangles[i + 0]];
        Vector3 b = mesh.vertices[mesh.triangles[i + 1]];
        Vector3 c = mesh.vertices[mesh.triangles[i + 2]];

        if (worldSpace)
        {
            return new Triangle(
                transform.TransformPoint(a),
                transform.TransformPoint(b),
                transform.TransformPoint(c));
        }
        else
        {
            return new Triangle(a, b, c);
        }
    }

    private static float CalculateAreaOfTrinagle(Triangle triangle)
    {
        Vector3 A = triangle.Item1;
        Vector3 B = triangle.Item2;
        Vector3 C = triangle.Item3;

        float res = Mathf.Pow(((B.x * A.y) - (C.x * A.y) - (A.x * B.y) + (C.x * B.y) + (A.x * C.y) - (B.x * C.y)), 2.0f);
        res += Mathf.Pow(((B.x * A.z) - (C.x * A.z) - (A.x * B.z) + (C.x * B.z) + (A.x * C.z) - (B.x * C.z)), 2.0f);
        res += Mathf.Pow(((B.y * A.z) - (C.y * A.z) - (A.y * B.z) + (C.y * B.z) + (A.y * C.z) - (B.y * C.z)), 2.0f);

        return Mathf.Sqrt(res) * 0.5f;
    }

    private int SampleTriangle()
    {
        float r = Random.Range(0, 1.0f);
        float p = 0.0f;

        for (int i = 0; i < probabilities.Count; i += 1)
        {
            p += probabilities[i];
            if (p > r)
                return i;
        }

        return -1;
    }

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        float sumTrisArea = 0.0f;

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Triangle triangle = GetTriangle(i, true);
            triangles.Add(triangle);
            sumTrisArea += CalculateAreaOfTrinagle(triangle);
        }


        for (int i = 0; i < triangles.Count; i += 1)
            probabilities.Add(CalculateAreaOfTrinagle(triangles[i]) / sumTrisArea);


        for (int i = 0; i < numGrains; i += 1)
        {
            int sample = SampleTriangle();
            Triangle triangle = triangles[sample];

            Vector3 randompoint = GetRandomPointInTriagnle(triangle);

            GameObject grain = GameObject.Instantiate(grainObjs[0], transform);
            grain.transform.position = randompoint;

            Vector3 triNormal = Vector3.Cross(triangle.Item2 - triangle.Item1, triangle.Item3 - triangle.Item1).normalized;

            grain.transform.up = triNormal;
        }
    }
}
