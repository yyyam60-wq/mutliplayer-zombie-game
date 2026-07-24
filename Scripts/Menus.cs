using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menus : MonoBehaviour
{
    public string MenuName = "";

    public void StartMenu(string Name) 
    {
        if (Name == MenuName) 
        {
            gameObject.SetActive(true);
        }
    }
}
