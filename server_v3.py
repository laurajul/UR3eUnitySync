import json
import time
import paho.mqtt.client as mqtt
from rtde_control import RTDEControlInterface
from rtde_receive import RTDEReceiveInterface
import socket
import threading

import time


def move_robot(joint_states):
    print("move_robot(): Starting to move robot to:", joint_states)
    rtde_c.moveJ(joint_states)
    print("move_robot(): Robot has reached the desired joint_states")
    mqtt_client.publish(topic_pub_JointCommandDone, "true")
    print("move_robot(): Published move complete")


def on_message(client, userdata, message):
    print("on_message(): Message received:", message.payload.decode())
    joint_states = list(map(float, message.payload.decode().split(',')))
    print("on_message(): Joint States parsed:", joint_states)
    threading.Thread(target=move_robot, args=(joint_states,)).start()
    print("on_message(): Thread started to move robot")


def establish_connections(max_attempts=3):
    global rtde_c, rtde_r, mqtt_client
    attempts = 0
    while attempts < max_attempts:
        try:
            print("establish_connections(): Attempting to connect to robot and MQTT broker")
            rtde_c = RTDEControlInterface(robot_ip)
            rtde_r = RTDEReceiveInterface(robot_ip)
            mqtt_client.connect(broker, port, 60)
            mqtt_client.subscribe(topic_sub_JointCommand)
            print("establish_connections(): Successfully connected to robot and MQTT broker")
            return True
        except socket.error:
            attempts += 1
            print("establish_connections(): Failed to connect to robot or MQTT broker. Retrying...")
            time.sleep(1)
    print("establish_connections(): Failed to establish connections after {} attempts.".format(max_attempts))
    print("establish_connections(): Make sure that the UR is in remote mode and/or MQTT Broker is running!")
    return False


broker = "127.0.0.1"
port = 1883
topic_pub_jointState = "ur_robot/joint_positions"
topic_sub_JointCommand = "ur_robot/joint_commands"
topic_pub_JointCommandDone = "ur_robot/move_complete"
mqtt_client = mqtt.Client("UR_Robot_Publisher")
mqtt_client.on_message = on_message


robot_ip = "10.0.0.3"
rtde_c = None
rtde_r = None


def main():
    print("main(): Entering main function")
    if not establish_connections():
        print("main(): Couldn't establish connections, exiting")
        return
    
    mqtt_client.loop_start()
    print("main(): MQTT loop started")

    while True:
        try:
       #     print("main(): Getting actual joint states from the robot")
            joints = rtde_r.getActualQ()
            joint_dict = {"joint1": joints[0], "joint2": joints[1], "joint3": joints[2], "joint4": joints[3], "joint5": joints[4], "joint6": joints[5]}
            print("main(): Received joint states:", joint_dict)
            
            mqtt_client.publish(topic_pub_jointState, json.dumps(joint_dict))
      #      print("main(): Published actual joint states")
            
            time.sleep(0.01)
        except (socket.error, ValueError) as e:
            print("main(): Exception occurred -", e)
            print("main(): Lost connection to robot or MQTT broker")
            
            if not establish_connections():
                mqtt_client.loop_stop()
                print("main(): Failed to re-establish connections, stopping MQTT loop and exiting main loop")
                break


if __name__ == "__main__":
    print("Script Starting")
    main()
