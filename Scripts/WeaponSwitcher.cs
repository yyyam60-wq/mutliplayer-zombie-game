using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityStandardAssets.CrossPlatformInput;

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

        CurrentWeapon = FirstWeapon;
        FirstWeapon.gameObject.SetActive(true);

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
