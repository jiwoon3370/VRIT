# <img src="https://raw.githubusercontent.com/Tarikul-Islam-Anik/Animated-Fluent-Emojis/master/Emojis/Handshakes/Hand%20with%20Fingers%20Splayed.png" width="35" /> VRIT: Virtual Reality Interaction Translator

<div align="center">

```text
██╗   ██╗██████╗ ██╗████████╗
██║   ██║██╔══██╗██║╚══██╔══╝
██║   ██║██████╔╝██║   ██║   
╚██╗ ██╔╝██╔══██╗██║   ██║   
 ╚████╔╝ ██║  ██║██║   ██║   
  ╚═══╝  ╚═╝  ╚═╝╚═╝   ╚═╝  
```

**Bridging the physical and virtual, one hand gesture at a time.**
*Created by nikmak45*

<br/>

[![Python 3.8+](https://img.shields.io/badge/Python-3.8+-3776AB?style=for-the-badge&logo=python&logoColor=white)](https://www.python.org/)
[![Unity 2021.3+](https://img.shields.io/badge/Unity-C%23-000000?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com/)
[![Protocol UDP](https://img.shields.io/badge/Protocol-UDP-f59e0b?style=for-the-badge)](https://en.wikipedia.org/wiki/User_Datagram_Protocol)
[![License MIT](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)](https://opensource.org/licenses/MIT)

<p align="center">
  VRIT는 별도의 특수 장비 없이 <b>일반 웹캠</b>만으로 사용자의 손동작을 가상 세계의 물리적 상호작용으로 변환하는 시스템입니다.<br/>
  MediaPipe를 통한 관절 추출부터 Unity 내 가속도 기반 던지기까지, 현실의 움직임을 디지털 데이터로 번역합니다.
</p>

</div>

---

## ✨ Key Features

| Feature | Description | Technical Detail |
| :--- | :--- | :--- |
| **01 Tracking** | Real-time Hand Tracking | MediaPipe를 통해 손의 21개 관절 포인트를 실시간 추출 |
| **02 Network** | Low Latency UDP | Python 서버와 Unity 클라이언트 간 초저지연 데이터 전송 |
| **03 Grab** | Precise Interaction | 엄지와 검지의 중점(Midpoint) 계산을 통한 정교한 집기 |
| **04 Throw** | Physical Dynamics | 최근 10프레임의 위치 변화량($v = \Delta s / \Delta t$) 기반 던지기 |

---

## 🛠️ Requirements & Setup

### 🐍 Python (Server)
- **Version**: 3.8+
- **Packages**: `mediapipe`, `opencv-python`

```bash
pip install mediapipe opencv-python
```

> [!TIP]
> **Hand Tracking Tip**: `hand_tracking.py`에서 `pixel_dist < 30` 조건으로 잡기 감도를 조절할 수 있습니다. 인식이 잘 안 된다면 값을 40~50으로 높여보세요.

### 🎮 Unity (Client)
- **Version**: 2021.3+ (LTS 추천)
- **Setup Flow**:
  1. **NetworkManager**: 빈 오브젝트 생성 후 `UDPReceiver.cs` 추가 (Port: 5005)
  2. **Hand Prefabs**: `LeftHand`, `RightHand` 생성 및 `VisualHandController.cs` 연결
  3. **Interaction**: 공(Ball) 오브젝트에 `Rigidbody` 추가 및 바닥(Plane) 구성

---

## 💡 Interaction Logic

### Throwing Mechanism
단순한 위치 고정이 아닌, 물리 법칙을 적용한 던지기를 구현합니다.

1. **Queue Tracking**: 매 프레임 손의 위치 데이터를 큐에 저장합니다.
2. **Velocity Compute**: 손을 펴는 순간(Release) 큐의 데이터를 분석하여 평균 속도 벡터를 계산합니다.
3. **Physics Injection**: 계산된 벡터를 `Rigidbody.velocity`에 직접 주입하여 자연스러운 포물선 운동을 완성합니다.

---

## 📂 Project Structure

```bash
VRIT/
├── python/
│   ├── hand_tracking.py      # MediaPipe 관절 추출 및 UDP 송신 로직
│   └── .venv/                # 가상 환경 (추천)
└── unity_project/
    └── Assets/
        └── Scripts/
            ├── HandData.cs             # 공용 데이터 구조 정의
            ├── UDPReceiver.cs          # 데이터 수신 로직
            └── VisualHandController.cs # 시각화 및 잡기/던지기 로직
```

---

<div align="center">

**MIT License** — Created for educational purposes. Free to use and improve.
**Contact**: [github: nikmak45](https://github.com/nikmak45)

</div>
