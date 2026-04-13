using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WebSocketSharp;

public class ItemsData : MonoBehaviour
{
    public static ItemsData instance;

    public Player player;

    public WeaponBase[] weapons;
    public WeaponBase weapon;

    public int price = 100;
    public string ComponentName = "";

    public void SetItem() 
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (!player.photonView.IsMine) return;

        weapons = FindObjectsOfType<WeaponBase>(true);
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].gameObject.name == ComponentName)
            {
                weapon = weapons[i];
            }
        }
    }
}
