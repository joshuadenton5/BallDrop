using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateHole : MonoBehaviour
{
    Mesh mesh;
    Maze maze;

    void Start()
    {
        mesh = transform.GetComponent<Mesh>();
    }

    void DeleteTriangle(int index)
    {
        Destroy(gameObject.GetComponent<MeshCollider>());
        Mesh mesh = transform.GetComponent<MeshFilter>().mesh;
        int[] oldTri = mesh.triangles;
        int[] newTri = new int[mesh.triangles.Length - 3];

        int i = 0;
        int j = 0;
        while(j < mesh.triangles.Length)
        {
            if (j != index * 3)
            {
                newTri[i++] = oldTri[j++];
                newTri[i++] = oldTri[j++];
                newTri[i++] = oldTri[j++];
            }
            else
                j += 3;           
        }
        transform.GetComponent<MeshFilter>().mesh.triangles = newTri;
        gameObject.AddComponent<MeshCollider>();
    }
}
