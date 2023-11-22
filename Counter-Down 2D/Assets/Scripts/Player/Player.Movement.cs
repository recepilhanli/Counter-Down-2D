

using System.Collections;
using Unity.Mathematics;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

public partial class Player : NetworkBehaviour
{

    [Header("Movement")]
    [SerializeField] Rigidbody2D _RigidBody;

    [SerializeField] ClientNetworkTransform _ClientNetworkTransform;

    private float _Speed = 150f;

    private Vector2 m_Velocity = Vector2.zero; //for ref

    Vector2 _AimedPosition = Vector2.zero;

    [SerializeField] Collider2D _Collider;

    [SerializeField] ClientNetworkTransform _ClientTransfrom;

    public static bool AutoOrientation = false;

    private Coroutine _StepCoroutine;

    private Vector2 LastInput = Vector2.zero;

    public void Teleport(Vector3 pos)
    {
        if (!IsOwner) return;

        if (_ClientNetworkTransform != null) _ClientNetworkTransform.Teleport(pos, quaternion.identity, new Vector3(1, 1, 1));
        if (_BotController == null) transform.position = pos;
        else
        {
            if (_BotController.Agent != null) _BotController.Agent.transform.position = pos;
            transform.position = pos;
        }
    }


    IEnumerator StepSound()
    {

        while (LastInput != Vector2.zero)
        {
            yield return new WaitForSeconds(0.2f);
            _StepSound?.PlaySound();
            yield return null;
        }
        _StepCoroutine = null;

        yield return null;
    }


    /// <summary>
    /// Gets input from the player
    /// </summary>
    void Movement()
    {

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (Money.Value >= 1000 && Armour.Value != 100)
            {
                _BuySound.PlaySound();
                BuyArmourServerRPC();
            }
        }

        var mousePos = UIManager.Instance.CrossTransfrom.position;
        transform.up = (mousePos - transform.position);

        if (Input.GetKey(KeyCode.LeftShift)) _Speed = 60;
        else _Speed = 150;


        var pos = Vector2.zero;
        if (AutoOrientation)
        {
            float dist = Vector2.Distance(mousePos, transform.position);
            if (dist < 0.75f) dist = 0; //prevent the 'o' effect
            else dist = Mathf.Clamp(dist, 7.5f, 10) / 3;
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) pos = new Vector2(1, 1) * _Speed * dist * Time.fixedDeltaTime * transform.up;
        }
        else
        {
            float dist = Vector2.Distance(mousePos, transform.position);
            dist = Mathf.Clamp(dist, 7.5f, 10) / 3;

            float x = Input.GetAxisRaw("Horizontal") * _Speed * dist * Time.fixedDeltaTime;
            float y = Input.GetAxisRaw("Vertical") * _Speed * dist * Time.fixedDeltaTime;
            pos = new Vector2(x, y);
        }
        LastInput = pos;
        Move(pos);
        if (pos != Vector2.zero && _ShakeCoroutine == null)
        {
            if (_StepCoroutine == null) _StepCoroutine = StartCoroutine(StepSound());
            PlayCameraShake(0.5f, 0.2f);
        }
        else if (_ShakeCoroutine == null) PlayCameraShake(0f, 0f);

    }


    /// <summary>
    /// Move Player
    /// </summary>
    /// <param name="pos">Additive position.</param>
    void Move(Vector2 pos)
    {
        _RigidBody.velocity = Vector2.SmoothDamp(_RigidBody.velocity, pos, ref m_Velocity, .05f);
        _RigidBody.velocity = Vector2.ClampMagnitude(_RigidBody.velocity, 10f);
    }

    /// <summary>
    /// Teleport Player
    /// </summary>
    /// <param name="pos">Teleportation Position.</param>
    void Teleport(Vector2 pos)
    {

    }

    void PlayCameraShake(float _a, float _f, float _t = 0)
    {

        _Perlin.m_AmplitudeGain = _a;
        _Perlin.m_FrequencyGain = _f;

        if (_t != 0)
        {
            if (_ShakeCoroutine != null) StopCoroutine(_ShakeCoroutine);
            _ShakeCoroutine = StartCoroutine(CameraShake(_t));
        }

    }

    IEnumerator CameraShake(float time)
    {
        yield return new WaitForSeconds(time);
        _Perlin.m_AmplitudeGain = 0;
        _Perlin.m_FrequencyGain = 0;
        _ShakeCoroutine = null;
        yield return null;
    }



}
