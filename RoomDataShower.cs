using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class RoomDataShower : MonoBehaviour
{    
    public Text text;

    RoomInfo info;

    public void SetUp(RoomInfo _info) 
    {
        info = _info;
        text.text = "Room Name: " + info.Name + " , Player(s): " + info.PlayerCount + " / " + _info.MaxPlayers;
    }

    public void OnClick() 
    {
        MainMenuManager.instance.JoinToFindRoom(info);
    }
}
