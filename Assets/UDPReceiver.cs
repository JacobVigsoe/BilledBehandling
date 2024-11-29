using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

public class UDPReceiver : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 5065; // Match with Python script

    public float scalingFactor = 1.0f; // Public variable to adjust the size of the hand

    private GameObject[] jointSpheres = new GameObject[21];
    private GameObject[] boneCylinders;
    private Vector3[] jointPositions = new Vector3[21];

    // Define connections between joints to represent bones
    private int[,] connections = new int[,]
    {
        {0,1}, {1,2}, {2,3}, {3,4},       // Thumb
        {0,5}, {5,6}, {6,7}, {7,8},       // Index finger
        {0,9}, {9,10}, {10,11}, {11,12},  // Middle finger
        {0,13}, {13,14}, {14,15}, {15,16},// Ring finger
        {0,17}, {17,18}, {18,19}, {19,20} // Little finger
    };

    // Indices of the fingertips (you can adjust this if needed)
    private int[] fingertipIndices = { 4, 8, 12, 16, 20 };

    // Reference to the pet GameObject
    public GameObject petObject; // Assign your pet (rabbit) GameObject in the Inspector

    void Start()
    {
        InitUDP();

        // Instantiate joint spheres
        for (int i = 0; i < 21; i++)
        {
            jointSpheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            jointSpheres[i].name = "Joint_" + i;
            jointSpheres[i].transform.localScale = Vector3.one * 0.01f * scalingFactor; // Adjust size based on scalingFactor

            // Add colliders to the fingertip joints
            if (System.Array.IndexOf(fingertipIndices, i) >= 0)
            {
                SphereCollider fingerCollider = jointSpheres[i].GetComponent<SphereCollider>();
                fingerCollider.isTrigger = true;

                // Add the HandCollider script to handle collisions
                HandCollider handColliderScript = jointSpheres[i].AddComponent<HandCollider>();
                handColliderScript.petObject = petObject;
            }
            else
            {
                // Disable the collider for non-fingertip joints to optimize performance
                Collider collider = jointSpheres[i].GetComponent<Collider>();
                if (collider != null)
                    collider.enabled = false;
            }
        }

        // Instantiate bone cylinders
        boneCylinders = new GameObject[connections.GetLength(0)];
        for (int i = 0; i < connections.GetLength(0); i++)
        {
            boneCylinders[i] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            boneCylinders[i].name = $"Bone_{connections[i, 0]}_{connections[i, 1]}";
            boneCylinders[i].transform.localScale = new Vector3(0.005f * scalingFactor, 0.005f * scalingFactor, 0.005f * scalingFactor); // Initial size adjusted

            // Disable colliders on bone cylinders to optimize performance
            Collider collider = boneCylinders[i].GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
        }
    }

    void InitUDP()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                byte[] data = client.Receive(ref anyIP);

                string dataText = Encoding.UTF8.GetString(data);
                float[] landmarkArray = JsonConvert.DeserializeObject<float[]>(dataText);

                // Update hand model on the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() => UpdateHandModel(landmarkArray));
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error receiving data: " + ex.Message);
            }
        }
    }

    void UpdateHandModel(float[] landmarkArray)
    {
        if (landmarkArray == null || landmarkArray.Length != 63) // 21 landmarks * 3 coordinates
        {
            Debug.LogWarning("Invalid landmark data received.");
            return;
        }

        // Update joint positions
        for (int i = 0; i < 21; i++)
        {
            float x = landmarkArray[i * 3];
            float y = landmarkArray[i * 3 + 1];
            float z = landmarkArray[i * 3 + 2];

            // Map the coordinates to Unity space
            Vector3 position = new Vector3(
                (x - 0.5f) * scalingFactor,
                -(y - 0.5f) * scalingFactor,
                -z * scalingFactor
            );

            jointPositions[i] = position;

            if (jointSpheres[i] != null)
            {
                jointSpheres[i].transform.localPosition = position;
            }
        }

        // Update bone cylinders
        for (int i = 0; i < boneCylinders.Length; i++)
        {
            int jointAIndex = connections[i, 0];
            int jointBIndex = connections[i, 1];

            Vector3 posA = jointPositions[jointAIndex];
            Vector3 posB = jointPositions[jointBIndex];

            if (boneCylinders[i] != null)
            {
                UpdateCylinder(boneCylinders[i], posA, posB);
            }
        }
    }

    void UpdateCylinder(GameObject cylinder, Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        // Position the cylinder between the two joints
        cylinder.transform.position = (start + end) / 2.0f;

        // Orient the cylinder to point from start to end
        cylinder.transform.up = direction.normalized;

        // Scale the cylinder to match the distance between joints
        float thickness = 0.005f * scalingFactor; // Adjust thickness based on scalingFactor
        cylinder.transform.localScale = new Vector3(thickness, distance / 2.0f, thickness);
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null)
            receiveThread.Abort();
        client.Close();
    }
}
