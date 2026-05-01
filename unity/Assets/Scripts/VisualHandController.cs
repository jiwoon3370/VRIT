using System;
using System.Collections.Generic;
using UnityEngine;

public class VisualHandController : MonoBehaviour
{
    public UDPReceiver networkManager;
    public GameObject ball; // 유니티 인스펙터에서 Ball을 드래그해서 넣어주세요.

    [Header("Hand Settings")]
    public string handType = "Right";
    public Color jointColor;
    public Color lineColor;

    [Header("Position & Scale")]
    public Vector3 positionOffset = new Vector3(0, 5f, 0);
    public float movementScale = 20f;
    public float grabThreshold = 1.5f; // 공과 얼마나 가까워야 잡을 수 있는지

    private Transform[] jointObjects;
    private LineRenderer[] fingerLines;
    private bool isHolding = false;

    private readonly int[][] fingerChains = new int[][] {
        new int[] {0, 1, 2, 3, 4}, new int[] {0, 5, 6, 7, 8},
        new int[] {9, 10, 11, 12}, new int[] {13, 14, 15, 16},
        new int[] {17, 18, 19, 20}, new int[] {0, 17}, new int[] {5, 9, 13, 17}
    };

    [Header("Throwing Settings")]
    public int velocitySampleCount = 10; // 최근 10프레임의 위치를 저장
    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    public float throwBoost = 1.5f; // 던지는 힘 보정 계수

    void Start()
    {
        // (머테리얼 및 관절 생성 코드는 이전과 동일)
        Material jointMat = new Material(Shader.Find("Unlit/Color"));
        jointMat.color = jointColor;
        Material lineMat = new Material(Shader.Find("Unlit/Color"));
        lineMat.color = lineColor;

        jointObjects = new Transform[21];
        for (int i = 0; i < 21; i++)
        {
            GameObject joint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            joint.name = handType + "_Joint_" + i;
            joint.transform.SetParent(this.transform);
            joint.transform.localScale = Vector3.one * 0.3f;
            joint.GetComponent<Renderer>().material = jointMat;
            Destroy(joint.GetComponent<Collider>()); // 스냅 방식에선 콜라이더 불필요
            jointObjects[i] = joint.transform;
        }

        fingerLines = new LineRenderer[fingerChains.Length];
        for (int i = 0; i < fingerChains.Length; i++)
        {
            GameObject lineObj = new GameObject("Line_" + i);
            lineObj.transform.SetParent(this.transform);
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMat;
            lr.startWidth = 0.08f; lr.endWidth = 0.05f;
            lr.positionCount = fingerChains[i].Length;
            fingerLines[i] = lr;
        }
    }

    void Update()
    {
        if (string.IsNullOrEmpty(networkManager.lastReceivedPacket)) return;

        HandPacket[] allHands = JsonHelper.FromJson<HandPacket>(networkManager.lastReceivedPacket);

        foreach (HandPacket hand in allHands)
        {
            if (hand.type == handType)
            {
                // 1. 관절 위치 업데이트
                for (int i = 0; i < 21; i++)
                {
                    Vector3 rawPos = new Vector3(
                        (hand.landmarks[i].x - 0.5f) * movementScale,
                        (0.5f - hand.landmarks[i].y) * (movementScale * 0.5f),
                        -hand.landmarks[i].z * movementScale
                    );
                    jointObjects[i].position = Vector3.Lerp(jointObjects[i].position, rawPos + positionOffset, Time.deltaTime * 35f);
                }
                UpdateFingerLines();

                // 2. 잡기 로직 (핵심)
                HandleGrab(hand.is_grabbing);
                break;
            }
        }
    }

    Vector3 CalculateThrowVelocity()
    {
        if (positionHistory.Count < 2) return Vector3.zero;

        Vector3[] positions = positionHistory.ToArray();
        Vector3 totalDisplacement = positions[positions.Length - 1] - positions[0];

        // 평균 속도 = 총 변위 / 걸린 시간
        // (Time.deltaTime * 데이터 개수)로 대략적인 시간 산출
        float deltaTime = Time.deltaTime * positionHistory.Count;

        return totalDisplacement / deltaTime;
    }

    void HandleGrab(bool isGrabbing)
    {
        if (ball == null) return;

        Vector3 thumbTip = jointObjects[4].position;
        Vector3 indexTip = jointObjects[8].position;
        Vector3 grabPoint = (thumbTip + indexTip) / 2f;

        float dist = Vector3.Distance(grabPoint, ball.transform.position);
        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (isGrabbing && (dist < grabThreshold || isHolding))
        {
            isHolding = true;
            rb.isKinematic = true;
            ball.transform.position = grabPoint;

            // --- 위치 기록 (속도 계산용) ---
            positionHistory.Enqueue(grabPoint);
            if (positionHistory.Count > velocitySampleCount)
                positionHistory.Dequeue();
        }
        else if (!isGrabbing && isHolding)
        {
            // --- 던지기 핵심 로직 ---
            Vector3 throwVelocity = CalculateThrowVelocity();

            isHolding = false;
            rb.isKinematic = false; // 물리 다시 켬

            // 계산된 속도를 공의 리지드바디에 주입
            rb.velocity = throwVelocity * throwBoost;

            positionHistory.Clear(); // 기록 초기화
        }
    }

    void UpdateFingerLines()
    {
        for (int i = 0; i < fingerChains.Length; i++)
        {
            for (int j = 0; j < fingerChains[i].Length; j++)
            {
                fingerLines[i].SetPosition(j, jointObjects[fingerChains[i][j]].position);
            }
        }
    }
}