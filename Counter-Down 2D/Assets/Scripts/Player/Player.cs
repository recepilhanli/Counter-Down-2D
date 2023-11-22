
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public partial class Player : NetworkBehaviour
{

    public static Player Instance { get; private set; } = null;
    [Header("Player")]
    [SerializeField] GameObject _NickCanvas;
    [SerializeField] TextMeshProUGUI _NickTMP;

    private Cinemachine.CinemachineVirtualCamera _VirtualCam;
    private Cinemachine.CinemachineBasicMultiChannelPerlin _Perlin;

    private Coroutine _ShakeCoroutine;

    public SpriteRenderer spriteRenderer;
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> PlayerID = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] BotController _BotController;

    [SerializeField] Light2D _CurrentPlayerLight;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Init();
        Debug.Log("a player/bot spawned.");
    }


    void Init()
    {
        if (_BotController == null)
        {
            Server.Instance.Players[OwnerClientId] = this;
            Debug.Log("Init Player: " + OwnerClientId);
        }

        if (IsServer)
        {
            if (_BotController == null)
            { 
                Debug.Log("Setting player ID: " + OwnerClientId);
                try
                {
                PlayerID.Value = (int)OwnerClientId;
                }
                catch
                {
                    Debug.Log("Something went wrong..");
                }
                Debug.Log("Set Player ID: " + PlayerID.Value);
            }
            PlayerTeam.Value = Teams.Team_None;
            Health.Value = 100;
            Armour.Value = 0;
        }

        if (!IsOwner)
        {
            Spawn();
        }
        else
        {
            if (_BotController == null)
            {

                PlayerName.Value = Server.Nickname;
                spriteRenderer.color = new Color(1, 1, 1, 0);
            }
            else PlayerName.Value = "Bot";
        }

    }

    IEnumerator GetBotID()
    {
        while (PlayerID.Value == 0)
        {
            Debug.Log("Waiting For Bot ID.. " + PlayerID.Value + " - " + gameObject.name);
            yield return null;
        }
        Debug.Log("Bot ID:" + (ulong)-PlayerID.Value);
        Server.Instance.Bots[(ulong)-PlayerID.Value] = this;
        yield return null;
    }

    Player FindPlayerFromID(short id)
    {
        var players = GameObject.FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            if (player.PlayerID.Value == id) return player;
        }
        return null;
    }

    void Start()
    {
        if (_BotController == null) Server.Instance.Players[OwnerClientId] = this;

        _NickCanvas.SetActive(false);

        if (IsServer)
        {
            BotController.players = GameObject.FindObjectsOfType<Player>();
            if (_BotController == null) PlayerID.Value = (int)OwnerClientId;
            Health.Value = 100;
        }

        if (_BotController != null)
        {
            Debug.Log("Getting bot ID..");
            StartCoroutine(GetBotID());
            name = $"Player(Bot {PlayerID.Value})";
        }

        if (IsOwner && _BotController == null)
        {
            _CurrentPlayerLight.enabled = true;
            name = "Player(Me)";
            Instance = this;
            spriteRenderer.sortingOrder = 10;
            _VirtualCam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
            _Perlin = _VirtualCam.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
            _VirtualCam.m_Follow = transform;
        }
        else _Collider.isTrigger = true;
    }




    void Update()
    {
        if (Instance == null) return;

        if (spriteRenderer.sprite == Server.Instance.TeamSprites[0] && PlayerTeam.Value == Teams.Team_T) spriteRenderer.sprite = Server.Instance.TeamSprites[1];
        else if (spriteRenderer.sprite == Server.Instance.TeamSprites[1] && PlayerTeam.Value == Teams.Team_CT) spriteRenderer.sprite = Server.Instance.TeamSprites[0];




        if ((!IsOwner && _BotController == null) || _BotController != null)
        {

            if (PlayerTeam.Value != Teams.Team_None)
            {
                float dist = Vector2.Distance(UIManager.Instance.CrossTransfrom.position, transform.position);

                bool sameTeam = (Instance.PlayerTeam.Value == PlayerTeam.Value) || Instance.PlayerTeam.Value == Teams.Team_None;

                if (sameTeam && _NickCanvas.activeInHierarchy == false)
                {
                    _NickTMP.color = Color.white;
                    _NickTMP.text = PlayerName.Value.ToString();
                    _NickCanvas.SetActive(true);
                }
                else if (!sameTeam && dist <= 2 && _NickCanvas.activeInHierarchy == false)
                {
                    _NickTMP.text = PlayerName.Value.ToString();
                    _NickTMP.color = Color.red;
                    _NickCanvas.SetActive(true);
                }
                else if (!sameTeam && dist > 2 && _NickCanvas.activeInHierarchy == true) _NickCanvas.SetActive(false);
            }
            return; //make sure this object belongs to current player who is playing
        }
        else if (PlayerTeam.Value == Teams.Team_None) return;
        Movement();
        Combat();
    }

}



