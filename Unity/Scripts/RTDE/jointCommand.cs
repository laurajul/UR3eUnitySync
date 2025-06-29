using System;
using System.Collections;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class jointCommand : MonoBehaviour
{
    // MQTT client object
    private MqttClient client;

    // User-defined MQTT broker IP
    public string mqttBrokerIP = "127.0.0.1";

    // MQTT topic for sending joint commands
    public string topic_JointCommand = "ur_robot/joint_commands";
    public string topic_moveComplete = "ur_robot/move_complete";

    // User-defined array of command strings
    public string[] CommandStrings;

    // Boolean flag to signal when a move is completed
    private bool isMoveComplete = true;

    private void Start()
    {
        // Initialize MQTT client
        client = new MqttClient(mqttBrokerIP);
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

        // Connect to the MQTT broker
        string clientId = Guid.NewGuid().ToString();
        client.Connect(clientId);

        // Subscribe to the "move_complete" topic
        client.Subscribe(new string[] { topic_moveComplete }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    private void Update()
    {
        // Start sending commands when Space key is pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SendJointCommands());
        }
    }

    // Coroutine for sending joint commands and handling wait commands
    IEnumerator SendJointCommands()
    {
        for (int i = 0; i < CommandStrings.Length; i++)
        {
            // Send joint command
            yield return new WaitUntil(() => isMoveComplete);
            string command = CommandStrings[i];

            if (command.StartsWith("wait"))  // Check if the command is a wait command
            {
                string[] parts = command.Split(',');
                float waitTime = float.Parse(parts[1]);
                yield return new WaitForSeconds(waitTime);
                isMoveComplete = true;
            }
            else
            {
                client.Publish(topic_JointCommand, System.Text.Encoding.UTF8.GetBytes(command), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
                isMoveComplete = false;
            }
        }
    }

    // This function is triggered when a message is received from the MQTT broker
    private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        // Check if the message received is "true", then set isMoveComplete to true
        string message = System.Text.Encoding.UTF8.GetString(e.Message);
        if (message == "true")
        {
            isMoveComplete = true;
        }
    }
}