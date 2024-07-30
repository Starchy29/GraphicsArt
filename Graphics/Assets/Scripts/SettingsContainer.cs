using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SimSettings {
    public static int BYTE_SIZE = sizeof(int) + 6 * sizeof(float);

    public int agentCount;
    public float moveSpeed; // pixels per second
    public float turnSpeed; // radians per second
    public float fadeRate;
    public float blurRate;
    public float senseRange;
    public float senseRotation; // radians
}

public static class SettingsContainer
{
    public static SimSettings Settings { get { return strands; } }

    private static SimSettings test = new SimSettings {
        agentCount = 100,
        moveSpeed = 50,
        turnSpeed = 4f,
        fadeRate = 0.2f,
        blurRate = 10f,
        senseRange = 60f,
        senseRotation = 0.4f,
    };

    private static SimSettings slow = new SimSettings {
        agentCount = 1000000,
        moveSpeed = 100f,
        turnSpeed = 4f,
        fadeRate = 0.2f,
        blurRate = 10f,
        senseRange = 30f,
        senseRotation = 0.2f,
    };

    private static SimSettings bloom = new SimSettings {
        agentCount = 1000000,
        moveSpeed = 250f,
        turnSpeed = 6f,
        fadeRate = 0.2f,
        blurRate = 10f,
        senseRange = 120f,
        senseRotation = 0.4f,
    };

    private static SimSettings milkVortex = new SimSettings {
        agentCount = 1000000,
        moveSpeed = 400f,
        turnSpeed = 20f,
        fadeRate = 0.2f,
        blurRate = 10f,
        senseRange = 100f,
        senseRotation = 0.3f,
    };

    private static SimSettings fuzzyChain = new SimSettings {
        agentCount = 1000000,
        moveSpeed = 1000f,
        turnSpeed = 100f,
        fadeRate = 0.2f,
        blurRate = 10f,
        senseRange = 100f,
        senseRotation = 0.8f,
    };

    private static SimSettings staticNoise = new SimSettings {
        agentCount = 1000000,
        moveSpeed = 2000f,
        turnSpeed = 200f,
        fadeRate = 0.6f,
        blurRate = 10f,
        senseRange = 100f, // 50, 100, 200
        senseRotation = 0.2f,
    };

    private static SimSettings spectral = new SimSettings {
        agentCount = 10000,
        moveSpeed = 5000f,
        turnSpeed = 200f,
        fadeRate = 0.2f,
        blurRate = 10f,
        senseRange = 100f,
        senseRotation = 0.2f,
    };

    private static SimSettings strands = new SimSettings {
        agentCount = 10000, // 10000, 20000
        moveSpeed = 10000f, // ridiculously high speed
        turnSpeed = 50f,
        fadeRate = 0.5f,
        blurRate = 10f,
        senseRange = 50f,
        senseRotation = 0.8f,
    };

    private static SimSettings creepy = new SimSettings {
        agentCount = 100000,
        moveSpeed = 700f,
        turnSpeed = 20f,
        fadeRate = 1.0f,
        blurRate = 0f,
        senseRange = 100f,
        senseRotation = 0.5f,
    };

    private static SimSettings consistent = new SimSettings {
        agentCount = 250000,
        moveSpeed = 20f,
        turnSpeed = 1f,
        fadeRate = 0.2f,
        blurRate = 3f,
        senseRange = 30f,
        senseRotation = 0.61f,
    };
}
