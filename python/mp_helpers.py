import cv2

def draw_custom_skeleton(image, hand_landmarks, connections, color):
    # 점 그리기
    for landmark in hand_landmarks.landmark:
        cx, cy = int(landmark.x * image.shape[1]), int(landmark.y * image.shape[0])
        cv2.circle(image, (cx, cy), 5, color, cv2.FILLED)
    
    # 선 그리기
    for connection in connections:
        start_idx = connection[0]
        end_idx = connection[1]
        
        start_lm = hand_landmarks.landmark[start_idx]
        end_lm = hand_landmarks.landmark[end_idx]
        
        start_pos = (int(start_lm.x * image.shape[1]), int(start_lm.y * image.shape[0]))
        end_pos = (int(end_lm.x * image.shape[1]), int(end_lm.y * image.shape[0]))
        
        cv2.line(image, start_pos, end_pos, color, 2)