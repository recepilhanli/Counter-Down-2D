using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance = null;

    [SerializeField] GameObject _HudCanvas;
    [SerializeField] GameObject _TeamSelectionCanvas;

    [Space]

    [SerializeField] TextMeshProUGUI _MoneyTMP;
    [SerializeField] TextMeshProUGUI _HealthTMP;
    [SerializeField] TextMeshProUGUI _ArmourTMP;
    [SerializeField] TextMeshProUGUI _AmmoTMP;
    [SerializeField] TextMeshProUGUI _KillTMP;

    [Space]

    [SerializeField] GameObject _DMG_UP;
    [SerializeField] GameObject _DMG_DOWN;
    [SerializeField] GameObject _DMG_RIGHT;
    [SerializeField] GameObject _DMG_LEFT;
    [SerializeField] GameObject _BuySign;

    [Space]

    public Transform CrossTransfrom;
    private Camera _MainCam;

    public void TeamSelection(int Team)
    {

        if (Team == 1 && Player.Instance.PlayerTeam.Value != Player.Teams.Team_T) Player.Instance.SelectTeamServerRPC(Player.Teams.Team_T);
        else if (Team == 0 && Player.Instance.PlayerTeam.Value != Player.Teams.Team_CT) Player.Instance.SelectTeamServerRPC(Player.Teams.Team_CT);

        _TeamSelectionCanvas.SetActive(false);
        _HudCanvas.SetActive(true);
        Cursor.visible = false;
    }

    public void ShowTeamSelection()
    {
        Debug.Log("Show");
        _TeamSelectionCanvas.SetActive(true);
    }


    public void DamageIndicator(Vector2 pos)
    {
        if (pos == Vector2.zero)
        {
            _DMG_UP.SetActive(false);
            _DMG_DOWN.SetActive(false);
            _DMG_RIGHT.SetActive(false);
            _DMG_LEFT.SetActive(false);
            return;
        }

        if (pos.x > 0.25f) _DMG_RIGHT.SetActive(true);
        else if (pos.x < -0.25f) _DMG_LEFT.SetActive(true);
        if (pos.y > 0.25f) _DMG_UP.SetActive(true);
        else if (pos.y < -0.25f) _DMG_DOWN.SetActive(true);

    }

    private void Awake()
    {
        _MainCam = FindAnyObjectByType<Camera>();
        Instance = this;
    }

    void Update()
    {
        if (Player.Instance == null) return;


        var mousePos = _MainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        CrossTransfrom.position = mousePos;

        float health = Player.Instance.Health.Value;
        float armour = Player.Instance.Armour.Value;
        ushort money = Player.Instance.Money.Value;
        ushort kills = Player.Instance.KillCount.Value;


        if (money >= 1000 && !_BuySign.activeInHierarchy) _BuySign.SetActive(true);
        else if (money < 1000 && _BuySign.activeInHierarchy) _BuySign.SetActive(false);

        if (health > 20) _HealthTMP.text = $"b{health.ToString("0")}";
        else _HealthTMP.text = $"<color=red>b{health.ToString("0")}";

        if (armour > 20) _ArmourTMP.text = $"a{armour.ToString("0")}";
        else _ArmourTMP.text = $"<color=red>a{armour.ToString("0")}";

        _MoneyTMP.text = $"${money}";
        _KillTMP.text = $"k{kills}";



        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_TeamSelectionCanvas.activeInHierarchy)
            {
                _TeamSelectionCanvas.SetActive(false);
                Cursor.visible = false;
                return;
            }
            else
            {
                DestroyImmediate(NetworkManager.Singleton.gameObject);
                SceneManager.LoadScene("Menu", LoadSceneMode.Single);
            }
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            _TeamSelectionCanvas.SetActive(!_TeamSelectionCanvas.activeInHierarchy);


            if (!_TeamSelectionCanvas.activeInHierarchy)
            {
                Cursor.visible = false;
            }
            else Cursor.visible = true;
        }

    }
}
