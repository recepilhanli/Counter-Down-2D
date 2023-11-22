

using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public partial class Player : NetworkBehaviour
{
    [Header("Combat")]

    [SerializeField] Light2D _FireLight;

    [Space]

    public NetworkVariable<Teams> PlayerTeam = new NetworkVariable<Teams>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> Health = new NetworkVariable<float>(default, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> Armour = new NetworkVariable<float>(default, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    public NetworkVariable<ushort> Money = new NetworkVariable<ushort>(default, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);
    public NetworkVariable<ushort> KillCount = new NetworkVariable<ushort>(default, NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> Spectator = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public enum Teams
    {
        Team_None,
        Team_CT,
        Team_T
    };



    void Combat()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AttackServerRPC();
        }
    }


    public void CreateCorpse()
    {
        if (PlayerTeam.Value == Teams.Team_None) return;

        if (PlayerTeam.Value == Teams.Team_CT) Instantiate(Server.Instance.Corpses[0], transform.position, Quaternion.identity);
        else Instantiate(Server.Instance.Corpses[1], transform.position, Quaternion.identity);
    }

    public void Spawn()
    {
        if (IsOwner)
        {

            if (_BotController == null) _VirtualCam.m_Lens.OrthographicSize = 7.5f;
            _NickCanvas.SetActive(false);
            GetSpawnPoint();
        }

        switch (PlayerTeam.Value)
        {
            case Teams.Team_CT:
                {
                    spriteRenderer.sprite = Server.Instance.TeamSprites[0];
                    var color = spriteRenderer.color;
                    color.a = 1;
                    spriteRenderer.color = color;
                    _SpawnSound?.PlaySound();
                    if (_BotController != null) Debug.Log("Bot Spawned");
                    break;
                }

            case Teams.Team_T:
                {
                    spriteRenderer.sprite = Server.Instance.TeamSprites[1];
                    var color = spriteRenderer.color;
                    color.a = 1;
                    spriteRenderer.color = color;
                    _SpawnSound?.PlaySound();
                    if (_BotController != null) Debug.Log("Bot Spawned");
                    break;
                }

            default:
                {
                    var color = spriteRenderer.color;
                    color.a = 0;
                    spriteRenderer.color = color;
                    _NickCanvas.SetActive(false);
                    break;
                }
        }
    }


    public void GetSpawnPoint()
    {
        if (!IsOwner) return;


        switch (PlayerTeam.Value)
        {

            case Teams.Team_CT:
                {
                    Transform randomTransform = Server.Instance.CTSpawnPoints[UnityEngine.Random.Range(0, Server.Instance.CTSpawnPoints.Count)];
                    var pos = randomTransform.position;
                    pos.z = 0;
                    Teleport(pos);
                    break;
                }


            case Teams.Team_T:
                {
                    Transform randomTransform = Server.Instance.TSpawnPoints[UnityEngine.Random.Range(0, Server.Instance.TSpawnPoints.Count)];
                    var pos = randomTransform.position;
                    pos.z = 0;
                    Teleport(pos);
                    break;
                }


            default: break;
        }

    }



    #region Client RPC

    void OnDeath()
    {
        _DeathSound?.PlaySound();
        CreateCorpse();
        Spawn();

        _BotController?.OnBotDeath();
    }

    [ClientRpc]
    public void OnDeathClientRPC()
    {
        OnDeath();
    }

    [ClientRpc]
    public void SpawnClientRPC()
    {
        StartCoroutine(FirstSpawn());
    }


    IEnumerator FirstSpawn()
    {
        while (PlayerTeam.Value == Teams.Team_None) //wait for server
        {
            yield return null;
        }
        Spawn();
        yield return null;
    }



    [ClientRpc]
    public void OnPlayerWeaponShootClientRPC()
    {
        _FireSound?.PlaySound();
        if (IsOwner && _BotController == null) PlayCameraShake(2.0f, 0.75f, 0.05f);
        StartCoroutine(FireEffect());
    }


    IEnumerator FireEffect()
    {
        Instantiate(Server.Instance.BulletTracePrefab, transform.position, transform.rotation);
        _FireLight.enabled = true;
        yield return new WaitForSeconds(0.05f);
        _FireLight.enabled = false;
        yield return null;
    }



    IEnumerator DamageEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = Color.white;
        yield return null;
    }


    IEnumerator DamageEffect(Vector2 pos)
    {
        if (IsOwner && _BotController == null)
        {
            PlayCameraShake(2, 0.75f, 0.075f);
            _DamageSound?.PlaySound();
            UIManager.Instance.DamageIndicator(pos);
        }

        var BloodParticle = Instantiate(Server.Instance.BulletParticle[1], transform.position, quaternion.identity);
        BloodParticle.transform.up = -pos;
        Destroy(BloodParticle, 2f);

        yield return new WaitForSeconds(0.05f);

        if (IsOwner && _BotController == null)
        {
            yield return new WaitForSeconds(0.1f);
            UIManager.Instance.DamageIndicator(Vector2.zero);
        }


        yield return null;
    }





    [ClientRpc]
    public void OnTakeDamageClientRPC(int attacker, bool isBot = false)
    {
        if (Instance.PlayerID.Value == attacker) Debug.Log("Player Attacked. " + (ulong)attacker);
        Player player = null;
        if (!isBot) player = Server.Instance.Players[(ulong)attacker];
        else player = Server.Instance.Bots[(ulong)-attacker];
        var attackNorm = (player.transform.position - transform.position);
        attackNorm.Normalize();
        StartCoroutine(DamageEffect(attackNorm));
        if (!IsOwner || _BotController != null)
        {
            StartCoroutine(DamageEffect());
        }
    }

    #endregion
    #region Server RPC


    [ServerRpc(RequireOwnership = true)]
    public void BuyArmourServerRPC()
    {
        if (Money.Value < 1000) return;
        if (Armour.Value == 100) return;

        Money.Value -= 1000;
        Armour.Value = 100;
    }


    [ServerRpc(RequireOwnership = true)]
    public void SelectTeamServerRPC(Teams team)
    {
        if (PlayerTeam.Value == Teams.Team_None)
        {
            SpawnClientRPC();
            PlayerTeam.Value = team;
        }
        else
        {
            PlayerTeam.Value = team;
            OnDeathClientRPC();
        }
    }


    [ServerRpc(RequireOwnership = true)]
    public void AttackServerRPC()
    {
        Attack();
    }

    public void Attack()
    {
        if (PlayerTeam.Value == Teams.Team_None || Spectator.Value == true) return;

        var hit = Physics2D.Raycast(transform.position + transform.up, transform.up, Mathf.Infinity, Server.Instance.DefaultLayer);
        if (hit)
        {

            var _player = hit.transform.GetComponent<Player>();

            if (_player != null)
            {

                if (PlayerTeam.Value != _player.PlayerTeam.Value && _player.PlayerTeam.Value != Teams.Team_None && _player.Spectator.Value == false)
                {

                    float dist = Vector2.Distance(transform.position, _player.transform.position);

                    float damage = 20 - dist / 1.5f;
                    damage = Mathf.Clamp(damage, 8, 20);

                    if (_player.Armour.Value > 0)
                    {
                        _player.Armour.Value -= damage;
                        _player.Armour.Value = Mathf.Clamp(_player.Armour.Value, 0, 100);
                        if (_player.Armour.Value > 0) _player.Health.Value -= damage / 2.5f;
                    }
                    else _player.Health.Value -= damage;

                    Money.Value += (ushort)(damage * 3);
                    Money.Value = (ushort)Mathf.Clamp(Money.Value, 0, 16000);

                    _player.OnTakeDamageClientRPC(PlayerID.Value, (_BotController != null));
                    _player._BotController?.OnBotTakeDamage(this);
                    _player.CheckHealth(this);
                }
            }

        }
        Debug.Log("Attack");
        OnPlayerWeaponShootClientRPC();
    }


    //only server
    public void CheckHealth(Player p)
    {
        if (!IsServer) throw new System.Exception("Clients can't use this function!");

        if (Health.Value <= 0)
        {
            p.KillCount.Value++;
            OnDeathClientRPC();
            if (_BotController != null && !IsHost) OnDeath();

            Health.Value = 100;
            Armour.Value = 0;
        }
    }
    #endregion
}
