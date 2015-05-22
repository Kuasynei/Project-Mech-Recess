using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    string registeredGameName = /*"VGP142_NetworkExample_Ali"*/"Project-Mech-Recess";
    private float requestLength = 3;
    HostData[] hosts = null;
    public GameObject player;


    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGUI()
    {

        if (Network.isServer)
        {
            return;
        }

        if (Network.isClient)
        {
            if (GUI.Button(new Rect(25, 25, 200, 30), "Spawn Player"))
            {
                SpawnPlayer();
            }
            return;
        }
        if (GUI.Button(new Rect(25, 25, 200, 30), "Start Server"))
        {
            StartServer();
        }

        if (GUI.Button(new Rect(25, 65, 200, 30), "Get Server List"))
        {
            StartCoroutine("GetServerList");
        }


        if (hosts != null && hosts.Length > 0)
        {
            if (GUI.Button(new Rect(25, 105, 200, 30), hosts[0].gameName))
            {
                print(Network.Connect(hosts[0]).ToString());
            }
        }


    }

    public IEnumerator GetServerList()
    {
        MasterServer.RequestHostList(registeredGameName);
        float timeStarted = Time.time;
        float timeEnd = Time.time + requestLength;

        while (Time.time < timeEnd)
        {
            hosts = MasterServer.PollHostList();
            yield return new WaitForEndOfFrame();
        }
    }

    private void StartServer()
    {
        Network.InitializeServer(16, 25002, false);
        MasterServer.RegisterHost(registeredGameName, /*"Networking Ali"*/"Mech Recess", /*"VGP142"*/"Recess");
    }

    void OnServerInitialized()
    {
        Debug.Log("Server Initialized");
        SpawnPlayer();
    }

    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.RegistrationSucceeded)
        {
            print("Registration Successful");
        }
        else
        {
            print(msEvent.ToString());
        }
    }

    void OnConnectedToServer()
    {
        print("OnConnectedToServer");
    }
    void OnDisconnectedFromServer(NetworkDisconnection dc)
    {
        print("OnDisconnectedFromServer--" + dc.ToString());
    }

    void FailedToConnect(NetworkConnectionError error)
    {
        print("FailedToConnect--" + error.ToString());
    }

    void FailedToConnectToMasterServer(NetworkConnectionError error)
    {
        print("FailedToConnectToMasterServer--" + error.ToString());
    }

    void OnNetworkInstantiate(NetworkMessageInfo info)
    {

        print("OnNetworkInstantiate--" + info.ToString());
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        //remove rpcs and destroy object
    }

    void OnApplicationQuit()
    {
        if (Network.isServer)
        {
            Network.Disconnect();
            MasterServer.UnregisterHost();
        }
    
    }

    private void SpawnPlayer()
    {
        Network.Instantiate(player, new Vector3(2, 3, 3), Quaternion.identity, 0);
    }

}
