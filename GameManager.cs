using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Manager;

public class GameManager : ManagerBase , Networking
{
    public static GameManager instance;
    private Stopwatch sw = new Stopwatch();

    internal int SpawnNumber = 0;

    public List<Player> AlivePlayersList = new List<Player>();
    public List<Player> DeadPlayersList = new List<Player>();

    public Transform[] PlayerSpawnPoint;

    public Text PingCounterText , TimerText;

    public Button EndGameButton;

    [Header("Multiplayer Weapons")]

    public Button ClassesButton;

    public Dropdown MainWeapon_Drop;
    public Dropdown SecondaryWeapon_Drop;

    private void Awake()
    {
        sw.Start();

        print(Application.targetFrameRate.ToString());

        roomState = RoomState.Playing;
        menus = FindObjectsOfType<Menus>(true);
        instance = this;

        StartGameRPC();
    }
        
    private void Update()
    {
        switch (roomState)
        {
            case RoomState.Playing:

                TimerText.text = sw.Elapsed.Seconds.ToString();

                if (gameMode == GameMode.Zombie)
                {
                    AlivePlayersList.RemoveAll(Player => Player == null);
                    if (AlivePlayersList.Count == 0)
                    {
                        sw.Stop();
                        roomState = RoomState.GameOver;
                        Player.instance.ShowDeadScreen();
                        instance.Invoke("CallAllPlayersDead", 5f);
                    }
                }

                StartCoroutine(UpdatePing());

                if (Input.GetKey(KeyCode.Tab)) 
                {
                    OpenMenu("Stats");
                }

                break;
        }
    }

    IEnumerator UpdatePing()
    {
        while (!PhotonNetwork.OfflineMode) 
        {
            yield return new WaitForSeconds(0.5f);
            PingCounterText.text = "Ping: " + PhotonNetwork.GetPing().ToString();
        }
    }

    // invoked 
    void CallAllPlayersDead()
    {
        switch (PhotonNetwork.OfflineMode)
        {
            case true:
                EndGameRPC();
                break;

            case false:
                photonView.RPC("EndGameRPC", RpcTarget.All);
                break;
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient) 
        {
            instance = this;
            EndGameButton.interactable = true;
        }
    }

    public void DisconnectFromRoom()
    {
        if (PhotonNetwork.OfflineMode) 
        {
            EndGameRPC();
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // Is Master client
            try
            {
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    PhotonNetwork.CurrentRoom.SetMasterClient(PhotonNetwork.MasterClient.GetNext());
                }
                Player.instance.photonView.RPC("CallRemoveFromListRPC", RpcTarget.All);
            }
            finally
            {
                PhotonNetwork.LeaveRoom();
            }
        }
        else
        {
            // Not master client
            Player.instance.photonView.RPC("CallRemoveFromListRPC", RpcTarget.All);
            PhotonNetwork.LeaveRoom();
        }
    }

    // End Game while Running even if Players Are not dead (ONLINE ONLY)
    public void EndGame() 
    {
        photonView.RPC("EndGameRPC", RpcTarget.AllBuffered);
    }

    public override void OnLeftRoom()
    {
        try
        {
            if (!PhotonNetwork.OfflineMode) PhotonNetwork.JoinLobby();
        }
        finally 
        {
            roomState = RoomState.Null;
            EndGameButton.interactable = false;
        }
    }

    // The game Ends Because all the players Are dead
    [PunRPC]
    public void EndGameRPC()
    {
        Player.instance.ShowDeadScreen();

        ZombieSpawner.instance.SetDefualtData();
        ZombieSpawner.instance.enabled = false;

        AlivePlayersList.Clear();
        DeadPlayersList.Clear();

        if (PhotonNetwork.OfflineMode)
        {
            roomState = RoomState.Null;
            PhotonNetwork.LeaveRoom();
            StartCoroutine(TransferBetScenes("Main Menu" , "Loading2"));
            Player.instance.ResetCanvasAnimator();
        }
        else 
        {
            List<Player> Players = new List<Player>(FindObjectsOfType<Player>());
            foreach (Player player in Players) Destroy(player.gameObject);

            List<Zombie> Zombies = new List<Zombie>(FindObjectsOfType<Zombie>());
            foreach (Zombie zombie in Zombies) Destroy(zombie.gameObject);
            
            roomState = RoomState.Idle;

            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;

            StartCoroutine(TransferBetScenes("Main Menu" , "Loading2"));
            Player.instance.ResetCanvasAnimator();
        }

        Time.timeScale = 1f;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // An RPC To start the Game (online and offline)
    [PunRPC]
    void StartGameRPC()
    {
        if (SpawnNumber == 4)SpawnNumber = 0;
        PhotonNetwork.Instantiate("Player", PlayerSpawnPoint[SpawnNumber].position, PlayerSpawnPoint[SpawnNumber].rotation);
        SpawnNumber++;
        ItemsData[] items = FindObjectsOfType<ItemsData>();
        foreach (ItemsData itemsData in items) itemsData.SetItem();

        ClassesButton.interactable = false;

        if (gameMode == GameMode.Zombie)
        {
            ZombieSpawner.instance.SetDefualtData();
            ZombieSpawner.instance.enabled = true;
            ZombieSpawner.instance.RedefineData();
        }
        else
        {
            Player.instance.AllowHit = true;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.OfflineMode = true;
        EndGameButton.gameObject.SetActive(false);
        OpenMenu("Offline Menu");
    }

    // Quick Menu
    public void ResumeGame()
    {
        Player.instance.ResetData();
        OpenMenu("PlayerUI");
    }

    public void SetClasses() 
    {
        Player.instance.Spawn();
        Player.instance.weaponSwitcher.SetClasses();
    }
}
