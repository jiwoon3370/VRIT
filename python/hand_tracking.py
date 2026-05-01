import cv2
import mediapipe as mp
import socket
import json

# UDP 설정
UDP_IP = "127.0.0.1"
UDP_PORT = 5005
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

mp_hands = mp.solutions.hands
hands = mp_hands.Hands(min_detection_confidence=0.7, min_tracking_confidence=0.7)
mp_draw = mp.solutions.drawing_utils

cap = cv2.VideoCapture(0)

while cap.isOpened():
    success, image = cap.read()
    if not success: break

    image = cv2.flip(image, 1)
    h, w, c = image.shape
    image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    results = hands.process(image_rgb)
    
    total_data = []
    # 손별 상태 초기화
    l_grabbing = False
    r_grabbing = False

    if results.multi_hand_landmarks and results.multi_handedness:
        for hand_landmarks, handedness in zip(results.multi_hand_landmarks, results.multi_handedness):
            hand_type = handedness.classification[0].label
            
            # 잡기 판정 (픽셀 거리)
            p4 = hand_landmarks.landmark[4]
            p8 = hand_landmarks.landmark[8]
            px4, py4 = p4.x * w, p4.y * h
            px8, py8 = p8.x * w, p8.y * h
            pixel_dist = ((px4 - px8)**2 + (py4 - py8)**2)**0.5
            
            is_grabbing = pixel_dist < 30 # 설정하신 30 기준
            
            if hand_type == "Left":
                l_grabbing = is_grabbing
                color = (0, 165, 255) # 주황
            else:
                r_grabbing = is_grabbing
                color = (0, 255, 0)   # 초록

            # 스켈레톤 그리기 (왼손 주황, 오른손 초록)
            color = (0, 165, 255) if hand_type == "Left" else (0, 255, 0)
            mp_draw.draw_landmarks(image, hand_landmarks, mp_hands.HAND_CONNECTIONS,
                                 mp_draw.DrawingSpec(color=color, thickness=2, circle_radius=2))

            all_landmarks = []
            for lm in hand_landmarks.landmark:
                all_landmarks.append({"x": float(lm.x), "y": float(lm.y), "z": float(lm.z)})
            
            total_data.append({
                "type": hand_type,
                "landmarks": all_landmarks,
                "is_grabbing": bool(is_grabbing)
            })

    # 데이터 전송
    sock.sendto(json.dumps(total_data).encode(), (UDP_IP, UDP_PORT))

    cv2.putText(image, f"L_GR: {l_grabbing}", (10, 50), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 120, 255), 2)
    cv2.putText(image, f"R_GR: {r_grabbing}", (10, 100), cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 255, 0), 2)
    cv2.imshow('VRIT Python', image)
    if cv2.waitKey(5) & 0xFF == 27: break

cap.release()
cv2.destroyAllWindows()