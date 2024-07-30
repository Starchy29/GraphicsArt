using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsManager : MonoBehaviour
{
    private const float SIM_SPEED = 1f;

    [SerializeField] private ComputeShader computeShader;

    private MeshRenderer meshRenderer;
    private MeshFilter filter;
    private RenderTexture texture;
    private RenderTexture postProcessTexture;

    const int RESOLUTION_HEIGHT = 1440; // 1440
    const float ASPECT_RATIO = 16f / 9f;
    const int RESOLUTION_WIDTH = (int)(ASPECT_RATIO * RESOLUTION_HEIGHT);

    private struct Agent {
        public const int BYTE_SIZE = 4 * sizeof(float);

        public Vector2 position;
        public Vector2 direction;
    }
    private Agent[] agents;

    ComputeBuffer agentBuffer;
    ComputeBuffer settingsBuffer;

    const int AGENT_KERNEL = 0;
    const int DIFFUSE_KERNEL = 1;
    const int FOLLOW_TRAIL_KERNEL = 2;
    private int agentGroupCount;
    private Vector2Int postProcessGroupCounts;

    const float CYCLE_DURATION = 10.0f;
    private float t;

    ~GraphicsManager() {
        agentBuffer.Release();
        settingsBuffer.Release();
    }

    void Start() {
        Camera camera = Camera.main;
        camera.transform.position = new Vector3(0, 0, -1f);
        transform.position = Vector3.zero;
        Vector2 worldDims = new Vector2(ASPECT_RATIO * camera.orthographicSize * 2f, camera.orthographicSize * 2f);

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

        // set up agents
        Vector2 middle = new Vector2(RESOLUTION_WIDTH, RESOLUTION_HEIGHT) / 2f;
        agents = new Agent[SettingsContainer.Settings.agentCount];
        for(int i = 0; i < agents.Length; i++) {
            float angle = Random.value * Mathf.PI * 2f;
            agents[i] = new Agent {
                //position = new Vector2(Random.Range(0, RESOLUTION_WIDTH), Random.Range(0, RESOLUTION_HEIGHT)),
                position = Random.insideUnitCircle * RESOLUTION_HEIGHT / 2f + middle,
                //position = middle,
                //direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))
            };

            agents[i].direction = (middle - agents[i].position).normalized; // move towards the center
        }

        // set up shader
        settingsBuffer = new ComputeBuffer(1, SimSettings.BYTE_SIZE);
        settingsBuffer.SetData(new SimSettings[1] { SettingsContainer.Settings });
        computeShader.SetBuffer(AGENT_KERNEL, "settings", settingsBuffer);
        computeShader.SetBuffer(DIFFUSE_KERNEL, "settings", settingsBuffer);
        computeShader.SetBuffer(FOLLOW_TRAIL_KERNEL, "settings", settingsBuffer);

        agentBuffer = new ComputeBuffer(agents.Length, Agent.BYTE_SIZE);
        agentBuffer.SetData(agents);
        computeShader.SetBuffer(AGENT_KERNEL, "_Agents", agentBuffer);
        computeShader.SetBuffer(FOLLOW_TRAIL_KERNEL, "_Agents", agentBuffer);

        postProcessTexture = new RenderTexture(texture);
        computeShader.SetInt("pixelWidth", texture.width);
        computeShader.SetInt("pixelHeight", texture.height);
        computeShader.SetTexture(AGENT_KERNEL, "_Texture", texture);
        computeShader.SetTexture(FOLLOW_TRAIL_KERNEL, "_Texture", texture);
        computeShader.SetTexture(DIFFUSE_KERNEL, "_Texture", texture);
        computeShader.SetTexture(DIFFUSE_KERNEL, "_PostProcessTexture", postProcessTexture);

        uint groupSize;
        computeShader.GetKernelThreadGroupSizes(AGENT_KERNEL, out groupSize, out _, out _);
        agentGroupCount = Mathf.CeilToInt((float)agents.Length / groupSize);

        uint groupX, groupY;
        computeShader.GetKernelThreadGroupSizes(DIFFUSE_KERNEL, out groupX, out groupY, out _);
        postProcessGroupCounts = new Vector2Int(Mathf.CeilToInt((float)RESOLUTION_WIDTH / groupX), Mathf.CeilToInt((float)RESOLUTION_HEIGHT / groupY));
    }

    void Update() {
        t += SIM_SPEED * Time.deltaTime / CYCLE_DURATION;
        t %= 1f;

        //Color currentColor = new Color(
        //    Mathf.Clamp01(2f - Mathf.Abs(6f * (t > 0.5f ? t - 1.0f : t))),
        //    Mathf.Clamp01(2f - Mathf.Abs(6f * (t - 1f / 3f))),
        //    Mathf.Clamp01(2f - Mathf.Abs(6f * (t - 2f / 3f)))
        //);
        Color currentColor = Color.white;

        computeShader.SetFloats("agentColor", currentColor.r, currentColor.g, currentColor.b);
        computeShader.SetFloat("deltaTime", SIM_SPEED * Time.deltaTime);
        computeShader.SetFloat("totalTime", Time.realtimeSinceStartup);

        computeShader.Dispatch(FOLLOW_TRAIL_KERNEL, agentGroupCount, 1, 1);
        computeShader.Dispatch(AGENT_KERNEL, agentGroupCount, 1, 1);

    }

    private void FixedUpdate() {
        computeShader.SetFloat("deltaTime", SIM_SPEED * Time.fixedDeltaTime);
        //computeShader.Dispatch(FOLLOW_TRAIL_KERNEL, agentGroupCount, 1, 1);
        //computeShader.Dispatch(AGENT_KERNEL, agentGroupCount, 1, 1);
        RunPostProcess(DIFFUSE_KERNEL);
        
    }

    private void RunPostProcess(int kernelIndex) {
        computeShader.Dispatch(kernelIndex, postProcessGroupCounts.x, postProcessGroupCounts.y, 1);
        Graphics.CopyTexture(postProcessTexture, texture);
    }
}