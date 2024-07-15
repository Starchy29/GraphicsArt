using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    [SerializeField] private ComputeShader computeShader;

    private MeshRenderer meshRenderer;
    private MeshFilter filter;
    private RenderTexture texture;
    private Vector2 worldDims;

    const float ASPECT_RATIO = 16f / 9f;
    const int RESOLUTION_HEIGHT = 90;
    const int RESOLUTION_WIDTH = (int)(ASPECT_RATIO * RESOLUTION_HEIGHT);

    private struct Agent {
        public const int BYTE_SIZE = 3 * sizeof(float);

        public float posX;
        public float posY;
        public float angle;
    }
    private Agent[] agents;

    ComputeBuffer agentBuffer;

    const int AGENT_KERNEL = 0;
    const int POST_PROCESS_KERNEL = 1;
    private int agentGroupCount;
    private Vector2Int drawGroupCounts;

    ~GraphicsManager() {
        agentBuffer.Release();
    }

    void Start() {
        Camera camera = Camera.main;
        camera.transform.position = new Vector3(0, 0, -1f);
        transform.position = Vector3.zero;
        worldDims = new Vector2(ASPECT_RATIO * camera.orthographicSize * 2f, camera.orthographicSize * 2f);

        // create rectangle mesh covering the screen
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[4] {
            new Vector3(-worldDims.x / 2f, -worldDims.y / 2f, 0), // bottom left
            new Vector3(worldDims.x / 2f, -worldDims.y / 2f, 0), // bottom right
            new Vector3(worldDims.x / 2f, worldDims.y / 2f, 0), // top right
            new Vector3(-worldDims.x / 2f, worldDims.y / 2f, 0) // top left
        };
        mesh.triangles = new int[6] { // clockwise winding order
            0, 3, 2,
            0, 2, 1
        };
        mesh.uv = new Vector2[4] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/SimpleShader"));

        filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        // Create texture
        texture = new RenderTexture(RESOLUTION_WIDTH, RESOLUTION_HEIGHT, 1);
        texture.filterMode = FilterMode.Point;
        texture.enableRandomWrite = true;
        meshRenderer.sharedMaterial.mainTexture = texture;

        // run shader
        computeShader.SetInt("pixelWidth", texture.width);
        computeShader.SetInt("pixelHeight", texture.height);
        computeShader.SetTexture(POST_PROCESS_KERNEL, "_Texture", texture);
        computeShader.SetTexture(AGENT_KERNEL, "_Texture", texture);

        SetupAgents();
        uint groupSize;
        computeShader.GetKernelThreadGroupSizes(AGENT_KERNEL, out groupSize, out _, out _);
        agentGroupCount = Mathf.CeilToInt((float)agents.Length / groupSize);
        //computeShader.Dispatch(AGENT_KERNEL, Mathf.CeilToInt((float)agents.Length / groupSize), 1, 1);

        uint groupX, groupY;
        computeShader.GetKernelThreadGroupSizes(POST_PROCESS_KERNEL, out groupX, out groupY, out _);
        drawGroupCounts = new Vector2Int(Mathf.CeilToInt((float)RESOLUTION_WIDTH / groupX), Mathf.CeilToInt((float)RESOLUTION_HEIGHT / groupY));

        //computeShader.Dispatch(0, texture.width / 8, texture.height / 8, 1);
    }

    void FixedUpdate() {
        computeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        computeShader.Dispatch(AGENT_KERNEL, agentGroupCount, 1, 1);
    }

    private void SetupAgents() {
        agents = new Agent[100];
        for(int i = 0; i < agents.Length; i++) {
            agents[i] = new Agent {
                posX = Random.Range(0, RESOLUTION_WIDTH), 
                posY = Random.Range(0, RESOLUTION_HEIGHT),
                angle = Random.value * Mathf.PI * 2f
            };
        }

        computeShader.SetInt("numAgents", agents.Length);

        agentBuffer = new ComputeBuffer(agents.Length, Agent.BYTE_SIZE);
        agentBuffer.SetData(agents);
        computeShader.SetBuffer(AGENT_KERNEL, "_Agents", agentBuffer);
    }
}
