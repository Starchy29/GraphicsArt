using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    MeshRenderer renderer;
    MeshFilter filter;
    Vector2 dimensions;

    void Start() {
        Camera camera = Camera.main;
        camera.transform.position = new Vector3(0, 0, -1f);
        transform.position = Vector3.zero;
        dimensions = new Vector2(16f/9f * camera.orthographicSize * 2f, camera.orthographicSize * 2f);

        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[4] {
            new Vector3(-dimensions.x / 2f, -dimensions.y / 2f, 0),
            new Vector3(dimensions.x / 2f, -dimensions.y / 2f, 0),
            new Vector3(dimensions.x / 2f, dimensions.y / 2f, 0),
            new Vector3(-dimensions.x / 2f, dimensions.y / 2f, 0)
        };
        mesh.triangles = new int[6] { // clockwise winding order
            0, 3, 2,
            0, 2, 1
        };

        renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = new Material(Shader.Find("Unlit/MainShader"));

        filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = mesh;
    }

    void Update() {
        
    }
}
