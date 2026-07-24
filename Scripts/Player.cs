using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using Photon.Pun;
using Manager;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    public static Player instance;

    public bool Head = false;
    private Collider HeadCollider;

    private FirstPersonController controller;
    private ShoppingManager shoppingManager;
    private GameObject SpectateCamPos;
    [HideInInspector] public Animator CanvasAnimator;
    [HideInInspector] public bool AllowHit;

    private GameObject GamePlayUI;

    private Vector3 SyncPos = Vector3.zero;
    private Quaternion SyncRot = Quaternion.identity;

    [HideInInspector] public bool isDead = false;
    private Slider HealthSlider, StaminaSlider;

    internal WeaponSwitcher weaponSwitcher;

    public Character PlayerCharacter;

    public GameObject PlayerCharacterHOLDER;
    public GameObject WeaponMover;
    public Animator CharacterAnimator;

    public Transform[] CharacterWeapons;

    public GameObject SparkPrefab;
    public GameObject BloodPrefab;

    public GameObject PlayerCamera;
    public GameObject SpectateCamera;

    public float Health = 100f;

    public int Points = 0;
    public Text PointsText, HealthText;

    private void Awake()
        {
        if (gameObject.CompareTag("ZombieHead"))
        {
            Head = true;
            return;
        }

        if (photonView.IsMine) instance = this;

        GameManager.instance.AlivePlayersList.Add(gameObject.GetComponent<Player>());

        SyncPos = transform.position;
        SyncRot = transform.rotation;
        }

    private void Start()
        {
        if (gameObject.CompareTag("ZombieHead"))
        {
            Head = true;
            HeadCollider = GetComponent<Collider>();
            return;
        }

            weaponSwitcher = FindObjectOfType<WeaponSwitcher>(true);

            controller = GetComponent<FirstPersonController>();
            shoppingManager = GetComponent<ShoppingManager>();
            CanvasAnimator = GameObject.FindGameObjectWithTag("PlayerUI").GetComponent<Animator>();
            GamePlayUI = GameObject.FindGameObjectWithTag("GamePlayUI");

            if (!photonView.IsMine)
            {
                controller.enabled = false;
                Destroy(PlayerCamera);
                Destroy(SpectateCamera);
                PlayerCharacterHOLDER.SetActive(true);
                return;
            }
            else
            {
                Destroy(PlayerCharacter);
            }

            HealthSlider = GameObject.FindGameObjectWithTag("HealthSlider").GetComponent<Slider>();
            StaminaSlider = GameObject.FindGameObjectWithTag("RunningSlider").GetComponent<Slider>();
            PointsText = GameObject.FindGameObjectWithTag("PointsText").GetComponent<Text>();
            SpectateCamPos = GameObject.FindGameObjectWithTag("CamPos");
            HealthText = HealthSlider.GetComponentInChildren<Text>();

        StaminaSlider.value = 1f;
        }

    public void Update()
    {
        if (Head) return;

        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, SyncPos, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, SyncRot, 0.1f);

            if (Health <= 0)
                {
                    GameManager.instance.AlivePlayersList.Remove(gameObject.GetComponent<Player>());
                    GameManager.instance.DeadPlayersList.Add(gameObject.GetComponent<Player>());
                }

            return;
        }

        StrengthSlider();
        PauseFUNCTION();

        HealthSlider.value = Health / 100;
        HealthText.text = "Health: " + Health.ToString();
        PointsText.text = "Point:" + Points;
    }

    [PunRPC]
    void SyncPlayerHealth(float Damage) => GetDamage(Damage);

    public void CallPlayerMoveAnimation(float Speed , bool State) 
    {
        photonView.RPC("SyncPlayerMoveAnimation", RpcTarget.Others , Speed , State);
    }

    [PunRPC]
    void SyncPlayerMoveAnimation(float Speed, bool State)
    {
        CharacterAnimator.SetFloat("Speed", Speed);
        CharacterAnimator.SetBool("Moving" , State);
    }

    internal void PauseFUNCTION()
        {
            if (ManagerBase.roomState != RoomState.GameOver && Input.GetKeyDown(KeyCode.Escape))
            {
                switch (photonView.IsMine)
                {
                    case true:

                        controller.StopPlayerMove = true;
                         weaponSwitcher.FirstWeapon.StopWeapon = true;
                        if (weaponSwitcher.SecondWeapon != null) weaponSwitcher.SecondWeapon.StopWeapon = true;
                        weaponSwitcher.enabled = false;
                        if (PhotonNetwork.OfflineMode) Time.timeScale = 0;
                        break;
                }

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                GameManager.instance.OpenMenu("Quick Menu");
            }
        }

    public void StrengthSlider()
    {
        if (!controller.IsWalking)
        {
            if (StaminaSlider.value == 0)
            {
                controller.SetSpeed(5);
            }
            else
            {
                controller.SetSpeed(10);
                StaminaSlider.value -= 0.1f * Time.deltaTime;
            }
        }
        else
        {
            controller.SetSpeed(10);
            StaminaSlider.value += 0.05f * Time.deltaTime;
        }
    }

    public void SetWeapon(WeaponBase weapon)
        {
            photonView.RPC("SetWeaponRPC", RpcTarget.Others, weapon.name);
        }

    [PunRPC]
    void SetWeaponRPC(string weapon)
        {
            CharacterAnimator.SetBool("Police9mm", false);
            CharacterAnimator.SetBool("Compact9mm", false);
            CharacterAnimator.SetBool("AK47", false);
            CharacterAnimator.SetBool("Magnum", false);
            CharacterAnimator.SetBool("UMP45", false);
            CharacterAnimator.SetBool("Shotgun", false);
            CharacterAnimator.SetTrigger("ChangeWeapon");

            if (weapon != null) CharacterAnimator.SetBool(weapon, true);

            for (int i = 0; i < CharacterWeapons.Length; i++)
            {
                CharacterWeapons[i].gameObject.SetActive(false);
                if (CharacterWeapons[i].gameObject.name == weapon)
                {
                    CharacterWeapons[i].gameObject.SetActive(true);
                }
            }
        }

    public void PlayFireAnimation()
        {
            photonView.RPC("PlayFireAnimationRPC", RpcTarget.Others);
        }

    [PunRPC]
    void PlayFireAnimationRPC()
        {
            CharacterAnimator.SetTrigger("Fire");
        }

    public void PlayReloadAnimation()
        {
            photonView.RPC("PlayReloadAnimationRPC", RpcTarget.Others);
        }

    [PunRPC]
    void PlayReloadAnimationRPC()
        {
            CharacterAnimator.SetTrigger("Reload");
        }

    public void GetDamage(float Damage)
        {
            if (!photonView.IsMine) return;

            Health -= Damage;
        CanvasAnimator.CrossFadeInFixedTime("GetDamagedAnimations", 0.1f);
        
        if (Health <= 0)
        {
            switch (ManagerBase.gameMode)
            {
                case Manager.GameMode.MultiPlayer:
                    DeadState();
                    break;

                case Manager.GameMode.Zombie:
                    DeadState();
                    break;
            }
        }
    }

    public void Spawn()
    {
            controller.StopPlayerMove = false;

            weaponSwitcher.FirstWeapon.StopWeapon = false;
            if (weaponSwitcher.SecondWeapon != null) weaponSwitcher.SecondWeapon.StopWeapon = false;
            weaponSwitcher.enabled = true;
            Health = 100f;
            isDead = false;

            print("player spawned");

            photonView.RPC("SpawnPlayerRPC", RpcTarget.All);

            GamePlayUI.SetActive(true);

            controller.enabled = true; shoppingManager.enabled = true;
            transform.position = GameManager.instance.PlayerSpawnPoint[GameManager.instance.SpawnNumber].position;

            PlayerCamera.gameObject.SetActive(true);
            WeaponMover.gameObject.SetActive(true);

            SpectateCamera.gameObject.SetActive(false);

        if (GameManager.gameMode == GameMode.MultiPlayer)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            GameManager.instance.ClassesButton.interactable = false;
            GameManager.instance.OpenMenu("PlayerUI");
        }
    }

    [PunRPC]
    void SpawnPlayerRPC() 
    {
        GameManager.instance.AlivePlayersList.Add(gameObject.GetComponent<Player>());
        GameManager.instance.DeadPlayersList.Remove(gameObject.GetComponent<Player>());
    }

    void DeadState() 
    {
        controller.StopPlayerMove = true;

        weaponSwitcher.FirstWeapon.StopWeapon = true;
        if (weaponSwitcher.SecondWeapon != null) weaponSwitcher.SecondWeapon.StopWeapon = true;
        weaponSwitcher.enabled = false;

        Health = 0f;
        isDead = true;

        print("player is dead");

        photonView.RPC("SyncPlayerListPOS", RpcTarget.All);

        GamePlayUI.SetActive(false);

        controller.enabled = false; shoppingManager.enabled = false;
        transform.position = SpectateCamPos.transform.position;

        PlayerCharacterHOLDER.SetActive(false);
        PlayerCamera.gameObject.SetActive(false);
        WeaponMover.gameObject.SetActive(false);

        SpectateCamera.gameObject.SetActive(true);

        if (GameManager.gameMode == GameMode.MultiPlayer)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            GameManager.instance.ClassesButton.interactable = true;
            GameManager.instance.OpenMenu("Classes");
        }
    }

    [PunRPC]
    void SyncPlayerListPOS()
    {
        GameManager.instance.AlivePlayersList.Remove(gameObject.GetComponent<Player>());
        GameManager.instance.DeadPlayersList.Add(gameObject.GetComponent<Player>());
    }

    public void ShowDeadScreen()
    {
        photonView.RPC("ShowDeadScreenRPC", RpcTarget.All);
    }

    [PunRPC]
    void ShowDeadScreenRPC()
    {
        CanvasAnimator.SetTrigger("Dead");
        Invoke("ResetCanvasAnimator", 5f);
    }

    public void ResetCanvasAnimator()
    {
        CanvasAnimator.Rebind(); CanvasAnimator.Update(0f);
    }

    public void ResetData()
    {
        controller.ResetPlayerMoveData();
        WeaponSwitcher weaponSwitcher = FindObjectOfType<WeaponSwitcher>(true);
        weaponSwitcher.FirstWeapon.StopWeapon = false;
        if (weaponSwitcher.SecondWeapon != null) weaponSwitcher.SecondWeapon.StopWeapon = false;
        weaponSwitcher.enabled = true;
        Time.timeScale = 1;
        ResetCanvasAnimator();
    }

    [PunRPC]
    void CallRemoveFromListRPC()
    {
        GameManager.instance.AlivePlayersList.Remove(gameObject.GetComponent<Player>());
        GameManager.instance.DeadPlayersList.Remove(gameObject.GetComponent<Player>());
        Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else if (stream.IsReading)
        {
            SyncPos = (Vector3)stream.ReceiveNext();
            SyncRot = (Quaternion)stream.ReceiveNext();
        }
    }
}