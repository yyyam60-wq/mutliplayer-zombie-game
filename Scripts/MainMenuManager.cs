using Manager;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainMenuManager : ManagerBase, Networking
{
    private string URL = "localhost:80";
    public Text URL_TEXT;

    public static MainMenuManager instance;

    public InputField CreateRoomNameField;

    public Text PlayerCountText;
    public Text LobbyRoomNameText;

    public Button StartGameButton;
    public Button OnlineButton;
    public Button UpdateButton;

    public Dropdown GameModeDropDown;

    public GameObject RoomDataShowerPrefab;
    public Transform RoomDataShowerSpawnPoint;

    public InputField CreateUserNameFIELD, CreatePasswordFIELD;
    public InputField logUserNameFIELD, LogPasswordFIELD;
    public Text CreateState, LogState;

    [Header("Game Settings")]
    
    public int FrameLimiter = 60;

    [System.Obsolete]
    private void Awake()
    {
        Application.targetFrameRate = FrameLimiter;

        if (!PhotonNetwork.OfflineMode) 
        {
            instance = this;
            OpenMenu("LobbyMenu");
        }

        UpdateButton.interactable = false;
        OnlineButton.interactable = false;

        PhotonNetwork.OfflineMode = true;
        menus = FindObjectsOfType<Menus>(true);
        instance = this;
    }

    void Update()
    {
        switch (roomState)
        {
            case RoomState.Idle:
                UpdateLobbyText();
                break;
        }
    }

    public void UpdateGame() =>
        Application.OpenURL(URL + "/users/Update Files");

    // Delete This function in future
    public void InsertHTTP(GameObject ThisMenu)
    {
        if (URL_TEXT.text != "") URL = URL_TEXT.text.ToString();
        print(URL);
        Destroy(ThisMenu);
        StartCoroutine(CheckForUpate());
        OpenMenu("Offline Menu");
    }

    [System.Obsolete]
    IEnumerator CheckForUpate()
    {
        string Version = Application.version.ToString();
        WWWForm form = new WWWForm();
        form.AddField("Version", Version);

        UnityWebRequest www = new UnityWebRequest();
        www = UnityWebRequest.Post(URL + "/users/CheckForUpdates.php", form);

        yield return www.SendWebRequest();

        if (www.isHttpError == true || www.isNetworkError == true)
        {
            UpdateButton.interactable = false;
            OnlineButton.interactable = false;
        }
        else CheckMessage(null, www);
    }

    public void ZombieMode()
    {
        PhotonNetwork.CreateRoom("OFFLINE");
        StartGame();
    }

    //[System.Obsolete]
    public void OnlineMode(Button thisbutton)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return;
        thisbutton.interactable = false;
        StartCoroutine(ConnectToSQL(thisbutton));
    }

    [System.Obsolete]
    IEnumerator ConnectToSQL(Button thisbutton)
    {
        StartCoroutine(CheckForUpate());

        UnityWebRequest www = new UnityWebRequest(URL);

        yield return www.SendWebRequest();

        if (www.isHttpError == true || www.isNetworkError == true)
        {
            thisbutton.interactable = true;
            yield break;
        }
        else
        {
            thisbutton.interactable = true;
            OpenMenu("Account");
        }
    }

    void ConnectToPhotonServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;

        CreateState.text = "";
        LogState.text = "";

        CreateUserNameFIELD.interactable = true; logUserNameFIELD.interactable = true;
        CreatePasswordFIELD.interactable = true; LogPasswordFIELD.interactable = true;

        CreateRoomNameField.text = "";
        CreatePasswordFIELD.text = "";
        LogPasswordFIELD.text = "";
        logUserNameFIELD.text = "";
    }

    void CheckMessage(Button ThisButton, UnityWebRequest www)
    {
        CreateState.text = "";
        LogState.text = "";

        switch (www.downloadHandler.text.ToString())
        {
            case "Same Version":
                UpdateButton.interactable = false;
                OnlineButton.interactable = true;
                break;

            case "diffrent Version Needs Update":
                UpdateButton.interactable = true;
                OnlineButton.interactable = false;
                break;

            case "All fields has to be filled":
                ThisButton.interactable = true;
                CreateUserNameFIELD.interactable = true; logUserNameFIELD.interactable = true;
                CreatePasswordFIELD.interactable = true; LogPasswordFIELD.interactable = true;

                CreateState.text = www.downloadHandler.text.ToString();
                LogState.text = www.downloadHandler.text.ToString();
                break;

            case "Logged In":
                ConnectToPhotonServer();
                ThisButton.interactable = true;
                break;

            case "Create Account":
                OpenMenu("LOG");
                ThisButton.interactable = true;
                CreateUserNameFIELD.interactable = true;
                CreatePasswordFIELD.interactable = true;
                CreateRoomNameField.text = "";
                CreatePasswordFIELD.text = "";
                break;

            case "UserName or Password Is Wrong":
                ThisButton.interactable = true;
                logUserNameFIELD.interactable = true;
                LogPasswordFIELD.interactable = true;

                LogState.text = www.downloadHandler.text.ToString();
                break;

            case "This Account Is Logged by another user":
                ThisButton.interactable = true;
                logUserNameFIELD.interactable = true;
                LogPasswordFIELD.interactable = true;

                LogState.text = www.downloadHandler.text.ToString();
                break;

            case "This Account Already Exits":
                ThisButton.interactable = true;
                CreateUserNameFIELD.interactable = true;
                CreatePasswordFIELD.interactable = true;

                CreateState.text = www.downloadHandler.text.ToString();
                break;
        }

        print(www.downloadHandler.text.ToString());
    }

    public void CallCreateAccount(Button ThisButton)
    {
        ThisButton.interactable = false;
        CreateUserNameFIELD.interactable = false;
        CreatePasswordFIELD.interactable = false;
        CreateState.text = "";
        StartCoroutine(CreateAccount(ThisButton));
    }

    IEnumerator CreateAccount(Button ThisButton)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserName", CreateUserNameFIELD.text.ToString());
        form.AddField("Password", CreatePasswordFIELD.text.ToString());
        form.AddField("LogState", "false".ToString());

        UnityWebRequest www = new UnityWebRequest();

        www = UnityWebRequest.Post(URL + "/users/CreateAccount.php", form);

        yield return www.SendWebRequest();

        CreateState.text = www.downloadHandler.text.ToString();

        CheckMessage(ThisButton, www);
    }

    public void CallLogInAccount(Button ThisButton)
    {
        ThisButton.interactable = false;
        logUserNameFIELD.interactable = false;
        LogPasswordFIELD.interactable = false;
        LogState.text = "";
        StartCoroutine(LogInAccount(ThisButton));
    }

    IEnumerator LogInAccount(Button ThisButton)
    {
        WWWForm form = new WWWForm();
        form.AddField("UserName", logUserNameFIELD.text.ToString());
        form.AddField("Password", LogPasswordFIELD.text.ToString());

        UnityWebRequest www = new UnityWebRequest();

        www = UnityWebRequest.Post(URL + "/users/LogIn.php", form);

        yield return www.SendWebRequest();

        CheckMessage(ThisButton, www);
    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.OfflineMode)
        {
            return;
        }
        else
        {
            PhotonNetwork.JoinLobby();
            OpenMenu("MainMenu");
            print("Connected to The Server");
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            instance = this;
            StartGameButton.gameObject.SetActive(true);
        }
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(CreateRoomNameField.text) || !string.IsNullOrWhiteSpace(CreateRoomNameField.text))
        {
            PhotonNetwork.LeaveLobby();
            RoomOptions roomOptions = new RoomOptions();
            if (GameModeDropDown.value == 1)
            {
                gameMode = GameMode.MultiPlayer;
                roomOptions.MaxPlayers = 12;
            }
            else
            {
                gameMode = GameMode.Zombie;
                roomOptions.MaxPlayers = 4;
            }
            roomOptions.CleanupCacheOnLeave = false;
            string NameFormula = CreateRoomNameField.text + $" , Game Mode : {GameModeDropDown.options[GameModeDropDown.value].text}";
            PhotonNetwork.CreateRoom(NameFormula, roomOptions);
            StartGameButton.gameObject.SetActive(true);
            LobbyRoomNameText.text = NameFormula;
        }
        else
        {
            print("Room Name Is not Suitable");
        }
    }

    public void JoinToFindRoom(RoomInfo info)
    {
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinRoom(info.Name.ToString());
        LobbyRoomNameText.text = info.Name.ToString();
    }

    public void RefreshRoomList()
    {
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in RoomDataShowerSpawnPoint)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            Instantiate(RoomDataShowerPrefab, RoomDataShowerSpawnPoint).GetComponent<RoomDataShower>().SetUp(roomList[i]);
        }
    }

    public override void OnJoinedRoom()
    {
        print("Joined Room");
        PlayerCountText.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount;
        OpenMenu("LobbyMenu");
        roomState = RoomState.Idle;
    }

    // Function to Call Upadate text of NO of players in the same room
    void UpdateLobbyText()
    {
        photonView.RPC("UpdateLobbyTextRPC", RpcTarget.All);
    }

    // An RPC to update how much players are in the same room
    [PunRPC]
    void UpdateLobbyTextRPC()
    {
        PlayerCountText.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount;
    }

    // Start The game
    public void StartGame()
    {
        StartGameButton.interactable = false;

        if (!PhotonNetwork.OfflineMode)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            photonView.RPC("StartGameRPC", RpcTarget.All, GameModeDropDown.value);
        }
        else StartGameRPC();

        StartGameButton.interactable = true;
    }

    // An RPC To start the Game (online and offline)
    [PunRPC]
    void StartGameRPC(int mode = 0)
    {
        StartCoroutine(TransferBetScenes("ZombieProtoType", "Loading"));
        if (mode == 0) gameMode = GameMode.Zombie;
        else gameMode = GameMode.MultiPlayer;
    }

    // Exiting The online Mode and move to the offline one
    public void ExitOnline()
    {
        PhotonNetwork.Disconnect();
    }

    public void DisconnectFromRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Is Master client
            try
            {
                if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1)
                {
                    PhotonNetwork.CurrentRoom.SetMasterClient(PhotonNetwork.MasterClient.GetNext());
                }
            }
            finally
            {
                StartGameButton.gameObject.SetActive(false);
                PhotonNetwork.LeaveRoom();
            }
        }
        else PhotonNetwork.LeaveRoom(); //not master client
    }

    // When the Player disconnect it could be intentioal or by an error
    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.OfflineMode = true;
        OpenMenu("Offline Menu");
    }
}
