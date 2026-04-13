using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class ShoppingManager : MonoBehaviourPunCallbacks
{
    private AudioSource audioSource;
    private ItemsData ComponentData;
    private Player player;

    public AudioClip PurchaseSuccess;
    public AudioClip PurchaseFailed;

    public WeaponSwitcher switcher;
    public Transform DetectShopPoint;
    public Text ShopText;

    private void Start()
    {
        if (!photonView.IsMine) return;

        audioSource = gameObject.GetComponent<AudioSource>();
        player = gameObject.GetComponent<Player>();
        ShopText = GameObject.FindGameObjectWithTag("ShopText").GetComponent<Text>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        ShopText.text = "";
        DetectShop();
    }

    void DetectShop() 
    {
        RaycastHit hit;
        if (Physics.Raycast(DetectShopPoint.position, DetectShopPoint.forward, out hit, 1.5f)) 
        {
            ComponentData = hit.transform.GetComponent<ItemsData>();
            if (ComponentData) 
            {
                ShopText.text = "Press F for " + ComponentData.ComponentName + " [Cost:" + ComponentData.price + "]";
                
                if (Input.GetKeyDown(KeyCode.F) && player.Points >= ComponentData.price && ComponentData.gameObject.CompareTag("AmmoShop"))
                {
                    BuyAmmo();
                }
                else if (Input.GetKeyDown(KeyCode.F) && player.Points >= ComponentData.price && ComponentData.gameObject.CompareTag("HealthShop")) 
                {
                    BuyRestoreHealth();
                }
                else if (Input.GetKeyDown(KeyCode.F) && player.Points >= ComponentData.price) 
                {
                    BuyWeapon(ComponentData.weapon);
                }
                else if (Input.GetKeyDown(KeyCode.F))
                {
                    audioSource.PlayOneShot(PurchaseFailed);
                }
            }
        }
    }

    void BuyWeapon(WeaponBase weapon)
    {
        if (switcher.FirstWeapon == weapon || switcher.SecondWeapon == weapon) 
        {
            audioSource.PlayOneShot(PurchaseFailed);
            return;
        }

        player.Points -= ComponentData.price;
        audioSource.PlayOneShot(PurchaseSuccess);

        WeaponBase[] Weapons = FindObjectsOfType<WeaponBase>(true);
        for (int i = 0; i < Weapons.Length;i++)
        {
            Weapons[i].gameObject.SetActive(false);
        }

        if (switcher.SecondWeapon == null) 
        {
            switcher.SecondWeapon = weapon;
        }
        else if (switcher.CurrentWeapon == switcher.FirstWeapon) 
        {
            switcher.FirstWeapon.ResetData();
            switcher.FirstWeapon = weapon;
        }
        else 
        {
            switcher.SecondWeapon.ResetData();
            switcher.SecondWeapon = weapon;
        }

        switcher.CurrentWeapon = weapon;
        weapon.gameObject.SetActive(true);
        Player.instance.SetWeapon(weapon);
    }

    void BuyAmmo()
    {
        if (switcher.CurrentWeapon.AmmoLeft == switcher.CurrentWeapon.MaxAmmo) 
        {
            audioSource.PlayOneShot(PurchaseFailed);
            return;
        }

        switcher.CurrentWeapon.AmmoLeft = switcher.CurrentWeapon.MaxAmmo;
        player.Points -= ComponentData.price;
        audioSource.PlayOneShot(PurchaseSuccess);
        ComponentData.price *= 2;
    }

    void BuyRestoreHealth() 
    {
        if (player.Health == 100f) 
        {
            audioSource.PlayOneShot(PurchaseFailed);
            return;
        }

        player.Health = 100f;
        player.Points -= ComponentData.price;
        audioSource.PlayOneShot(PurchaseSuccess);
        ComponentData.price *= 2;
    }
}
