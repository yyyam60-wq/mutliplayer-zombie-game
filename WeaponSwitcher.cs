using Manager;

using System.Linq;

using UnityEngine;


public class WeaponSwitcher : MonoBehaviour
{
    private Animator animator;
    private bool ChangeingWeapon = false;

    public WeaponBase FirstWeapon;
    public WeaponBase SecondWeapon;

    public WeaponBase CurrentWeapon;

    private void Start()
    {
        animator = GetComponent<Animator>();

        SetClasses();
    }

    internal void SetClasses()
    {
        if (GameManager.gameMode == GameMode.MultiPlayer)
        {
            FirstWeapon?.gameObject.SetActive(false);
            SecondWeapon?.gameObject.SetActive(false);

            FirstWeapon?.ResetData();
            SecondWeapon?.ResetData();

            var weapons = FindObjectsOfType<WeaponBase>(true);

            FirstWeapon = weapons.First(x => x.name ==
            GameManager.instance.MainWeapon_Drop.options[GameManager.instance.MainWeapon_Drop.value].text);

            SecondWeapon = weapons.First(x => x.name ==
            GameManager.instance.SecondaryWeapon_Drop.options[GameManager.instance.SecondaryWeapon_Drop.value].text);
            SecondWeapon.gameObject.SetActive(false);

            CurrentWeapon = FirstWeapon;
            FirstWeapon.gameObject.SetActive(true);
        }
        else 
        {
            CurrentWeapon = FirstWeapon;
            FirstWeapon.gameObject.SetActive(true);
        }

        Invoke("SetWeaponOnStart", 2f);
    }

    void SetWeaponOnStart() 
    {
        Player.instance.SetWeapon(CurrentWeapon);
    }

    private void Update()
    {
        if (ChangeingWeapon || SecondWeapon == null) return;

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            ChangeingWeapon = true;
            animator.CrossFadeInFixedTime("ChangeWeaponAnimation", 0.1f);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            ChangeingWeapon = true;
            animator.CrossFadeInFixedTime("ChangeWeaponAnimation", 0.1f);
        }
    }

    public void ChangeWeapon()
    {
        CurrentWeapon.gameObject.SetActive(false);
        CurrentWeapon.enabled = true;

        if (CurrentWeapon == FirstWeapon) 
        {
            CurrentWeapon = SecondWeapon;
        }
        else 
        {
            CurrentWeapon = FirstWeapon;
        }

        DisableWeapon();

        CurrentWeapon.gameObject.SetActive(true);

        ChangeingWeapon = false;

        Player.instance.SetWeapon(CurrentWeapon);
    }

    private void EnableWeapon() 
    {
        CurrentWeapon.enabled = true;
    }

    private void DisableWeapon()
    {
        CurrentWeapon.enabled = false;
    }
}
