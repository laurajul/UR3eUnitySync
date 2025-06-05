# UR3e Robot Sync with Unity via MQTT

This script allows a UR3e (or other UR series) robot to be controlled in real-time using MQTT messages sent from external software like **Unity**. The robot receives joint commands and sends back its current joint positions continuously, enabling two-way synchronization between a simulation/game engine and the physical robot.

---

## Features

- Subscribe to joint commands via MQTT (`ur_robot/joint_commands`)
- Publish actual joint states via MQTT (`ur_robot/joint_positions`)
- Publish movement completion status (`ur_robot/move_complete`)
- Threaded robot motion execution using `RTDEControlInterface`
- Robust reconnect logic for robot and broker

---

## Use Case

This script was used in a setup where the Unity game engine simulated robot movements. Unity sent target joint values over MQTT, and the robot followed in real time. The robot also streamed back its actual joint positions to Unity, allowing for precise synchronization.

---

## Requirements

### Python Packages

Install the required dependencies using:

```bash
pip install paho-mqtt ur-rtde
````

### System Dependencies (if needed)

For systems like Raspberry Pi:

```bash
sudo apt update
sudo apt install cmake g++ libboost-all-dev python3-dev
```

If `ur-rtde` fails to install via pip, refer to the official documentation:
[https://sdurobotics.gitlab.io/ur\_rtde/](https://sdurobotics.gitlab.io/ur_rtde/)

---

## MQTT Topics

| Topic                      | Direction | Description                                                              |
| -------------------------- | --------- | ------------------------------------------------------------------------ |
| `ur_robot/joint_commands`  | subscribe | Receives joint positions as comma-separated values (`q1,q2,q3,q4,q5,q6`) |
| `ur_robot/joint_positions` | publish   | Sends actual joint positions as JSON                                     |
| `ur_robot/move_complete`   | publish   | Sends `"true"` after completing a movement                               |

---

## Configuration

In the script, adjust these variables as needed:

```python
broker = "127.0.0.1"         # MQTT broker IP (e.g., localhost or remote)
robot_ip = "10.0.0.3"        # UR robot IP
```

Make sure the robot is in **remote control mode** and the MQTT broker is running.

---

## Usage

1. Start your MQTT broker (e.g. Mosquitto).
2. Run this script on the robot controller host (e.g. Raspberry Pi):

```bash
python main.py
```

3. From Unity (or any MQTT client), publish a joint command like:

```
Topic: ur_robot/joint_commands
Message: 0.0,-1.57,1.57,-1.57,-1.57,0.0
```

4. The robot will move to those joint angles and publish when it finishes.
5. The robot's current joint state is continuously published for feedback.

---

## Notes

* Uses threaded motion to avoid blocking the MQTT client loop.
* Includes automatic reconnection attempts to both the robot and MQTT broker.
* Intended for use in a local network with low latency.



## Acknowledgments

Originally written by [Cihan Subaşı](https://github.com/chnsbs)
Adapted for public sharing and reuse.

