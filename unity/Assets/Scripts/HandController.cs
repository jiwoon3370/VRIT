using UnityEngine;
using System;

[Serializable]
public class HandData
{
    public float x, y, z;
    public int is_grabbing;
}

public class HandController : MonoBehaviour
{
    public UDPReceiver networkManager;
    public GameObject ball;
    public float grabDistance = 1.5f; // 타겟팅 범위
    private bool isHolding = false;

    void Update()
    {
        if (string.IsNullOrEmpty(networkManager.lastReceivedPacket)) return;

        // JSON 파싱 (간단하게 첫 번째 손 데이터만 사용)
        string json = networkManager.lastReceivedPacket;
        HandData[] hands = JsonHelper.FromJson<HandData>(json);

        if (hands.Length > 0)
        {
            // 1. 좌표 변환 (0~1 범위를 유니티 월드 좌표로 스케일업)
            Vector3 targetPos = new Vector3((hands[0].x - 0.5f) * 20, (0.5f - hands[0].y) * 10, 0);
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 15f);

            // 2. 로직 처리
            float distToBall = Vector3.Distance(transform.position, ball.transform.position);

            if (hands[0].is_grabbing == 1)
            {
                // 거리가 가깝거나 이미 잡고 있는 상태라면
                if (distToBall < grabDistance || isHolding)
                {
                    isHolding = true;
                    ball.GetComponent<Rigidbody>().isKinematic = true; // 물리 엔진 일시 정지
                    ball.transform.position = transform.position; // 9번 랜드마크 위치로 공 강제 이동
                }
            }
            else
            {
                if (isHolding)
                {
                    isHolding = false;
                    ball.GetComponent<Rigidbody>().isKinematic = false; // 다시 물리 적용
                }
            }
        }
    }
}