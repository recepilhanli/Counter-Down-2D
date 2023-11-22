using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Server : NetworkBehaviour
{

    public static byte ServerState = 2; //0 client connection, 1 create host, 2 create server
    public static byte NumberOfBots = 4;

    public static string Nickname = "Player";
    public static string ServerIP = "127.0.0.1";


    public static ushort ServerPort = 8888;

    public static Server Instance { get; private set; }
    [HideInInspector] public NetworkVariable<GameModes> GameModeNetworkVar = new NetworkVariable<GameModes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Dictionary<ulong, Player> Players = new Dictionary<ulong, Player>();
    public Dictionary<ulong, Player> Bots = new Dictionary<ulong, Player>();

    public GameModes GameMode = GameModes.GM_None;


    [Space]
    public GameObject BulletTracePrefab;

    public GameObject[] BulletParticle = new GameObject[2]; //0: impact, 1: blood


    public GameObject[] Corpses = new GameObject[2];

    public Sprite[] TeamSprites = new Sprite[2];

    [SerializeField] GameObject _BotPrefab;

    public LayerMask DefaultLayer;
    // Start is called before the first frame update

    public bool Started = false;
    float _WaitDuration = 0;

    [Space]

    public List<Transform> CTSpawnPoints = new List<Transform>();

    [Space]

    public List<Transform> TSpawnPoints = new List<Transform>();

    public enum GameModes
    {
        GM_None,
        GM_Deathmatch,
        GM_Defuse,
        GM_Rescue
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            GameModeNetworkVar.Value = GameMode;

            if (NumberOfBots > 0)
            {

                for (int i = 0; i < NumberOfBots / 2; i++)
                {
                    var go = Spawn(_BotPrefab, Vector2.zero, quaternion.identity);
                    go.GetComponentInChildren<BotController>().SetBotTeam(Player.Teams.Team_T);
                }
                for (int i = 0; i < NumberOfBots / 2; i++)
                {
                    var go = Spawn(_BotPrefab, Vector2.zero, quaternion.identity);
                    go.GetComponentInChildren<BotController>().SetBotTeam(Player.Teams.Team_CT);
                }

            }
        }

        Debug.Log($"The Game Started with {NumberOfBots} bots.");
        Started = true;
    }

    IEnumerator Start()
    {
        _WaitDuration = 10f + Time.time;
        Instance = this;
        Physics2D.GetIgnoreLayerCollision(6, 7);
        Physics2D.GetIgnoreLayerCollision(7, 6);
        Physics2D.GetIgnoreLayerCollision(7, 7);

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ServerIP;
        transport.ConnectionData.Port = ServerPort;

        if (ServerState == 0) NetworkManager.Singleton.StartClient();
        if (ServerState == 1) NetworkManager.Singleton.StartHost();
        if (ServerState == 2) NetworkManager.Singleton.StartServer();

        while (Started == false && (IsHost || IsClient))
        {
            yield return null;
        }
        UIManager.Instance.ShowTeamSelection();
        yield return null;
    }


    void Update()
    {
        if (Started == false && _WaitDuration < Time.time && _WaitDuration != 0)
        {
            DestroyImmediate(NetworkManager.Singleton.gameObject);
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
    }

    public void Kick(ulong clientid)
    {
        NetworkManager.Singleton.DisconnectClient(clientid);
    }

    public void Destroy(GameObject g)
    {
        var n = g.GetComponent<NetworkObject>();
        if (n != null)
        {
            if (n.IsSpawned) n.Despawn();
        }
    }

    public GameObject Spawn(GameObject instance, Vector3 p, Quaternion r)
    {
        var g = Instantiate(instance, p, r);
        var n = g.GetComponent<NetworkObject>();

        if (n != null)
        {
            if (!n.IsSpawned) n.Spawn();
        }
        return g;
    }

}
