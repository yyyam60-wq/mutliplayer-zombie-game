using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

namespace Manager
{
    public enum RoomState
    {
        Null,
        Idle,
        Playing,
        GameOver
    }

    public enum GameMode
    {
        Zombie, MultiPlayer,
    }

    public class ManagerBase : MonoBehaviourPunCallbacks
    {
        public static RoomState roomState = RoomState.Null;
        public static GameMode gameMode = GameMode.Zombie;

        [HideInInspector]public Menus[] menus;

        public void OpenMenu(string NameOfMenu)
        {
            foreach (Menus menus in menus)
            {
                menus.gameObject.SetActive(false);

                if (NameOfMenu == "Back" && PhotonNetwork.OfflineMode)
                {
                    menus.StartMenu("Offline Menu");
                } else if (NameOfMenu == "Back") menus.StartMenu("MainMenu"); // online panel or menu will be opened
                else menus.StartMenu(NameOfMenu); // start a normal menu
            }
        }

        public IEnumerator TransferBetScenes(string SceneName , string LoadingScreenName)
        {
            OpenMenu(LoadingScreenName);
            yield return new WaitForSeconds(10f);
            SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
        }
    }

    interface Networking 
    {
        void DisconnectFromRoom();
    }
}

