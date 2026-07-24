using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDIFF : WeaponBase
{
    public override void PlayFireAnimations()
    {
        if (AmmoInClip > 1)
        {
            animator.CrossFadeInFixedTime("Fire", 0.1f);
        }
        else
        {
            animator.CrossFadeInFixedTime("FireLast", 0.1f);
        }
    }

    public override void DryFire()
    {
        FireLock = true;
        audioSource.PlayOneShot(DryFireSound);
        StartCoroutine(NextTimeToFire());
        animator.CrossFadeInFixedTime("StandEmpty", 0.1f);
    }
}
