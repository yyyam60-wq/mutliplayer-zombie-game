using Manager;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public enum FireMode 
{
    SemiAuto,
    Auto
}

public class WeaponBase : MonoBehaviour
{
    protected Animator animator;
    protected AudioSource audioSource;
    protected FirstPersonController controller;
    protected Player player;
    private Transform ShootPoint;
    protected bool FireLock = false;
    protected bool Reloading = false;

    [HideInInspector] public bool StopWeapon;
    private WeaponBob weaponBob;
    private WeaponSway weaponSway;

    [Header("Audio References")]
    public AudioClip DrawSound;
    public AudioClip FireSound;
    public AudioClip DryFireSound;
    public AudioClip MagOutSound;
    public AudioClip MagInSound;
    public AudioClip OnBoltSound;

    [Header("UI References")]

    public Text WeaponNameText;
    public Text AmmoText;

    [Header("Weapon References")]

    public ParticleSystem MuzzleFlash;
    public GameObject SparkPrefab;
    public GameObject BloodPrefab;

    public FireMode fireMode = FireMode.SemiAuto;

    public float Damage = 20f;

    public float FireRate = 0.25f;

    public int pellets = 1;

    public int MaxAmmo = 100;
    public int ClipSize = 8;
    public int AmmoLeft;
    public int AmmoInClip;

    public float Range = 1000f;
    public float Spread = 0.1f;
    public float recoil = 0.5f;

    private void Awake()
    {
        AmmoText = GameObject.FindGameObjectWithTag("AmmoText").GetComponent<Text>();
        WeaponNameText = GameObject.FindGameObjectWithTag("WeaponNameText").GetComponent<Text>();

        AmmoLeft = MaxAmmo;
        AmmoInClip = ClipSize;
    }

    private void Start()
    {
        FireLock = true;

        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        ShootPoint = GameObject.FindGameObjectWithTag("ShootPoint").transform;
        weaponBob = GetComponent<WeaponBob>();
        weaponSway = GetComponent<WeaponSway>();

        animator.CrossFadeInFixedTime("Draw", 0.1f);

        Invoke("CanShoot",1.5f);
    }

    private void Update()
    {
        switch (StopWeapon)
        {
            case false:
                weaponBob.enabled = true;
                weaponSway.enabled = true;
                break;

            case true:
                weaponBob.enabled = false;
                weaponSway.enabled = false;
                return;
        }

        AmmoText.text = AmmoInClip + "/" + AmmoLeft;

        if (Input.GetButtonDown("Fire1") && fireMode == FireMode.SemiAuto)
        {
            CheckFire();
        }
        else if (Input.GetButton("Fire1") && fireMode == FireMode.Auto)
        {
            CheckFire();
        }

        if (Input.GetButton("Reload"))
        {
            CheckReload();
        }
    }

    public void ResetData()
    {

    }

    private void OnEnable()
    {
        FireLock = false;
        WeaponNameText.text = gameObject.name;
    }

    void CanShoot() 
    {
        FireLock = false;
    }

    public void CheckFire()
    {
        if (Reloading) return;
        if (FireLock) return;
        if (AmmoInClip == 0) 
        {
            DryFire();
            return;
        } 

        Fire();
    }

    public void Fire() 
    {
        FireLock = true;

        Recoil();

        PlayFireAnimations();

        audioSource.PlayOneShot(FireSound);

        for (int i = 0; i < pellets; i++) 
        {
            DetectHit();
        }

        Player.instance.PlayFireAnimation();
        MuzzleFlash.Stop();
        MuzzleFlash.Play();

        AmmoInClip--;

        StartCoroutine(NextTimeToFire());
    }

    public void Recoil() 
    {
        controller.mouseLook.Recoil(recoil);
    }

    public virtual void PlayFireAnimations() 
    {
        if (AmmoInClip >= 1)
        {
            animator.CrossFadeInFixedTime("Fire", 0.1f);
        }
    }

    public virtual void DryFire() 
    {
        FireLock = true;
        audioSource.PlayOneShot(DryFireSound);
        StartCoroutine(NextTimeToFire());
    }

    public void DetectHit() 
    {
        RaycastHit hit;
        switch (ManagerBase.gameMode) 
        {
            case Manager.GameMode.MultiPlayer:
                if (Physics.Raycast(ShootPoint.position, SpreadCalc(Spread, ShootPoint), out hit, Range))
                {
                    Player Health;
                    Health = hit.transform.GetComponent<Player>();
                    if (!Health || Health.gameObject.Equals(Player.instance.gameObject)) 
                    {
                        GameObject Spark = Instantiate(SparkPrefab, hit.point, hit.transform.rotation);
                        Destroy(Spark, 1f);
                    }
                    else if(Health.CompareTag("ZombieHead"))
                    {
                        Health.photonView.RPC("SyncPlayerHealth", RpcTarget.All, (Damage * 2));
                        player.Points += 20;
                        CreateBlood(hit);
                    }
                    else
                    {
                        Health.photonView.RPC("SyncPlayerHealth", RpcTarget.All, Damage);
                        player.Points += 10;
                        CreateBlood(hit);
                    }
                }
                break;
    
            case Manager.GameMode.Zombie:
                if (Physics.Raycast(ShootPoint.position, SpreadCalc(Spread, ShootPoint), out hit, Range, 7))
                {
                    Zombie Health;
                    Health = hit.transform.GetComponent<Zombie>();
                    if (!Health) 
                    {
                        GameObject Spark = Instantiate(SparkPrefab, hit.point, hit.transform.rotation);
                        Destroy(Spark, 1f);
                    }
                    else if (Health.CompareTag("ZombieHead"))
                    {
                        Health.photonView.RPC("SyncZombieHealth", RpcTarget.All, (Damage * 2));
                        player.Points += 20;
                        CreateBlood(hit);
                    }
                    else 
                    {
                        Health.photonView.RPC("SyncZombieHealth", RpcTarget.All, Damage);
                        player.Points += 10;
                        CreateBlood(hit);
                    }
                }
                break;
        }
    }
    
    void CreateBlood(RaycastHit hit) 
    {
        GameObject Blood = Instantiate(BloodPrefab, hit.point, transform.rotation);
        Destroy(Blood, 1f);
    }
    
    Vector3 SpreadCalc(float Spread, Transform ShootPoint) 
    {
        return Vector3.Lerp(ShootPoint.TransformDirection(Vector3.forward * 100), Random.onUnitSphere,Spread);
    }

    public virtual IEnumerator NextTimeToFire() 
    {
        yield return new WaitForSeconds(FireRate);
        FireLock = false;
    }

    public virtual void CheckReload() 
    {
        if (Reloading) return;
        if (AmmoInClip == ClipSize)return;
        if (AmmoLeft == 0)return;

        player.GetComponentInChildren<WeaponSwitcher>().enabled = false;
        player.GetComponent<ShoppingManager>().enabled = false;

        Player.instance.PlayReloadAnimation();
        Reloading = true;
        animator.CrossFadeInFixedTime("Reload",0.1f);
    }

    public virtual void ReloadCalc() 
    {
        int AmmosNeeded = ClipSize - AmmoInClip;
        if (AmmosNeeded > AmmoLeft) 
        {
            AmmoInClip += AmmoLeft;
            AmmoLeft = 0;
        }
        else 
        {
            AmmoInClip += AmmosNeeded;
            AmmoLeft -= AmmosNeeded;
        }
    }


    public void ReloadEnd()
    {
        player.GetComponentInChildren<WeaponSwitcher>().enabled = true;
        player.GetComponent<ShoppingManager>().enabled = true;
        Reloading = false;
    }

    void OnDraw() 
    {
        audioSource.PlayOneShot(DrawSound);
    }

    void OnMagOut()
    {
        audioSource.PlayOneShot(MagOutSound);
    }

    void OnMagIn()
    {
        audioSource.PlayOneShot(MagInSound);
        ReloadCalc();
    }

    void OnBoltForwarded()
    {
        audioSource.PlayOneShot(OnBoltSound);
    }
}
