using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class BotController : NetworkBehaviour
{
    private static int TotalBot = 0;
    private Vector2 _DestPos = Vector2.zero;
    private Coroutine _CombatCoroutine = null;
    [HideInInspector] public NavMeshAgent Agent = null;

    [HideInInspector] public Player FocuedPlayer = null;

    [SerializeField] Player _Player;

    public static Player[] players = null;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            TotalBot++;
            _Player.PlayerTeam.Value = Player.Teams.Team_CT;
            _Player.PlayerID.Value = -TotalBot;

            //bot will drive the navmesh agent
            var Created = new GameObject("BotNavAgent: " + -TotalBot);
            Created.transform.position = transform.position;
            Agent = Created.AddComponent<NavMeshAgent>();

            Agent.speed = 6;
        }

        StartCoroutine(WaitForSpawn());

    }


    public void SetBotTeam(Player.Teams team)
    {
        _Player.PlayerTeam.Value = team;
        _Player.Spawn();
    }



    IEnumerator WaitForSpawn()
    {
        while (_Player.PlayerTeam.Value == Player.Teams.Team_None) yield return null;
        _Player.Spawn();
        yield return null;
    }




    void RandomWanderingPosition()
    {
        Transform randomTransform = null;
        if (_Player.PlayerTeam.Value == Player.Teams.Team_CT) randomTransform = Server.Instance.TSpawnPoints[UnityEngine.Random.Range(0, Server.Instance.TSpawnPoints.Count)];
        else randomTransform = Server.Instance.CTSpawnPoints[UnityEngine.Random.Range(0, Server.Instance.CTSpawnPoints.Count)];
        var pos = randomTransform.position;
        pos.z = 0;
        _DestPos = pos;
        SetDest(pos);
    }

    IEnumerator BotCombat()
    {
        while (FocuedPlayer != null)
        {
            yield return new WaitForSeconds(0.75f + Random.Range(0, 0.15f));
            if (FocuedPlayer == null)
            {
                if (_CombatCoroutine != null) StopCoroutine(_CombatCoroutine);
                _CombatCoroutine = null;
                break;
            }
            float dist = Vector2.Distance(transform.position, FocuedPlayer.transform.position);
            if (dist <= 8.25f)
            {
                if (_Player.IsClient) Debug.Log("Bot Attacked to Player");
                _Player.Attack();
            }
            else FocuedPlayer = null;
            yield return null;
        }
        _CombatCoroutine = null;
        yield return null;
    }


    void Wandering()
    {
        if (FocuedPlayer == null)
        {

            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            for (int i = 0; i < players.Length; i++)
            {
                var player = players[i];
                if (player == null) continue;

                if (_Player.PlayerTeam.Value == player.PlayerTeam.Value || player.PlayerTeam.Value == Player.Teams.Team_None) continue;

                if (Vector2.Distance(transform.position, player.transform.position) < 8)
                {
                    Debug.Log($"Bot enemy player detection: {_Player.gameObject} + {player.gameObject}");
                    FocuedPlayer = player;
                    if (_CombatCoroutine == null) _CombatCoroutine = StartCoroutine(BotCombat());
                }
            }

        }

        if (FocuedPlayer != null)
        {
            if (_Player.PlayerTeam.Value == FocuedPlayer.PlayerTeam.Value || FocuedPlayer.PlayerTeam.Value == Player.Teams.Team_None) FocuedPlayer = null;
            else
            {
                float dist = Vector2.Distance(transform.position, FocuedPlayer.transform.position);
                if (dist > 1.25f) SetDest(FocuedPlayer.transform.position);
                else Agent.isStopped = true;
            }
            return;
        }

        else if (Agent.remainingDistance < 5 || !Agent.hasPath) RandomWanderingPosition();

    }

    public void OnBotTakeDamage(Player attacker)
    {
        if (FocuedPlayer == null) FocuedPlayer = attacker;
        else
        {
            float dist1 = Vector2.Distance(transform.position, FocuedPlayer.transform.position);
            float dist2 = Vector2.Distance(transform.position, attacker.transform.position);
            if (dist2 < dist1) FocuedPlayer = attacker;
            if (_CombatCoroutine == null && FocuedPlayer != null) _CombatCoroutine = StartCoroutine(BotCombat());
        }
    }


    void SetDest(Vector3 pos)
    {
        Agent.isStopped = false;
        Agent.SetDestination(pos);
    }

    public void OnBotDeath()
    {
        StartCoroutine(OnDeath());
    }

    IEnumerator OnDeath()
    {
        yield return new WaitForSeconds(1);
        FocuedPlayer = null;
        yield return null;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (_Player.PlayerTeam.Value == Player.Teams.Team_None) return;


        Vector3 pos = Agent.transform.position;
        pos.z = 0;
        _Player.transform.position = pos;
        if (FocuedPlayer == null)
        {
            pos = (Agent.destination - transform.position);
            pos.z = 0;
            pos.Normalize();
            transform.parent.up = pos;
        }
        else
        {
            pos = (FocuedPlayer.transform.position - transform.position);
            pos.Normalize();
            transform.parent.up = pos;
        }



        // vec3 = transform.position;
        // vec3.z = 0;
        // transform.position = vec3;

        if (!IsServer) return;


        Wandering();
    }

}
