


using Unity.Netcode;
using UnityEngine;


public partial class Player : NetworkBehaviour
{
    [Header("Sounds")]
    [SerializeField] SoundManager _FireSound;
    [SerializeField] SoundManager _StepSound;
    [SerializeField] SoundManager _SpawnSound;
    [SerializeField] SoundManager _DamageSound;
    [SerializeField] SoundManager _DeathSound;
    [SerializeField] SoundManager _BuySound;
}
