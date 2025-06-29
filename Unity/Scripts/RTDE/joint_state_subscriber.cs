using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;

public class joint_state_subscriber : MonoBehaviour
{
    private MqttClient client;


    private ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

    public GameObject[] joints;  // Define your joints array here
    public string mqttBrokerIP = "127.0.0.1";  // MQTT broker IP
    public string topic = "ur_robot/joint_positions";

    private void Start()
    {
        client = new MqttClient(mqttBrokerIP);
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
        client.Connect(Guid.NewGuid().ToString());
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string joint_json = System.Text.Encoding.UTF8.GetString(e.Message);
        var jointData = JsonUtility.FromJson<JointData>(joint_json);

        actions.Enqueue(() => UpdateJoints(jointData));
    }

    private void Update()
    {
        while (actions.Count > 0)
        {
            if (actions.TryDequeue(out var action))
            {
                action();
            }
        }
    }

    private void UpdateJoints(JointData jointData)
    {
        // assuming the joints array matches the order in which data is coming
        joints[0].transform.localRotation = Quaternion.Euler(0.0f, -jointData.joint1 * (180.0f / Mathf.PI), 0.0f);
        joints[1].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, jointData.joint2 * (180.0f / Mathf.PI) + 90.0f);
        joints[2].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, jointData.joint3 * (180.0f / Mathf.PI));
        joints[3].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, jointData.joint4 * (180.0f / Mathf.PI) + 90.0f);
        joints[4].transform.localRotation = Quaternion.Euler(0.0f, -jointData.joint5 * (180.0f / Mathf.PI), 0.0f);
        joints[5].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, jointData.joint6 * (180.0f / Mathf.PI));
    }

    [Serializable]
    public class JointData
    {
        public float joint1;
        public float joint2;
        public float joint3;
        public float joint4;
        public float joint5;
        public float joint6;
    }
}
