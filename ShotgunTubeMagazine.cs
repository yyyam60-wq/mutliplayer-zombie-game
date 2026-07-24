using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunTubeMagazine : WeaponBase
{
    public AudioClip Insert;

    public override void CheckReload()
    {
        if (Reloading) return;
        if (AmmoInClip == ClipSize) return;
        if (AmmoLeft == 0) return;

        player.GetComponentInChildren<WeaponSwitcher>().enabled = false;
        player.GetComponent<ShoppingManager>().enabled = false;

        Reloading = true;

        if (AmmoInClip > 0) 
        {
            animator.CrossFadeInFixedTime("ReloadStart",0.1f);
        }
        else
        {
            animator.CrossFadeInFixedTime("ReloadStartEmpty",0.1f);
        }
    }

    public void InsertAmmo() 
    {
        if (AmmoLeft == 0)
        {
            animator.CrossFadeInFixedTime("ReloadEnd", 0.1f);
            return;
        }

        AmmoLeft--;
        AmmoInClip++;

        audioSource.PlayOneShot(Insert); 
       
        if (AmmoInClip == ClipSize)
        {
            animator.CrossFadeInFixedTime("ReloadEnd", 0.1f);
        }
    }
}
