using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private float m_Range = 20f;
    [SerializeField] private float m_VerticalRange = 20f;
    [Space]
    [SerializeField] private AudioClip m_FireSound;
    [Space]
    [SerializeField] private LayerMask m_RaycastLayer;

    [SerializeField] private Transform m_ProjectileSpawn;

    private bool m_AllowInvoke = true, m_ReadyToShoot, m_Shooting;
    private int m_BulletsShot;

    private GunStats m_GunStats;
    private GunUI m_Gun;
    private BoxCollider m_GunTrigger; 
    private List<Damageable> m_Enemies = new List<Damageable>();

    public bool ReadyToShoot { get => m_ReadyToShoot; }
    public Transform ProjectileSpawn { get => m_ProjectileSpawn; }

    private void Awake()
    {
        m_ReadyToShoot = true;

        m_ProjectileSpawn = transform.Find("Projectile_Spawn");

        m_GunTrigger = GetComponent<BoxCollider>();
        m_GunTrigger.size = new Vector3(1, m_VerticalRange, m_Range);
        m_GunTrigger.center = new Vector3(0, 0, m_Range * 0.5f);
    }

    private void Update()
    {
        GetInput();
    }

    public void UpdateGunStats(GunStats stats, GunUI gun)
    {
        m_Gun = gun;
        m_GunStats = stats;
    }

    private void GetInput()
    {
        if (m_GunStats.allowButtonHold)
        {
            m_Shooting = Input.GetButton("Fire1");
        }
        else
        {
            m_Shooting = Input.GetButtonDown("Fire1");
        }

        if (m_ReadyToShoot && m_Shooting && m_GunStats.totalAmmos > 0)
        {
            m_BulletsShot = 0;
            Shoot();
        }
        else if (m_GunStats.totalAmmos <= 0)
        {
            // TO DO
            // Switch weapon
        }
    }

    private void Shoot()
    {
        m_ReadyToShoot = false;
        m_Gun.Shoot();

        if (m_GunStats.projectile == null)
        {
            foreach(Damageable damageable in m_Enemies)
            {
                Vector3 dir = damageable.transform.position - transform.position;
                RaycastHit hit;
                if (Physics.Raycast(transform.position, dir, out hit, m_Range * 1.5f, m_RaycastLayer))
                {
                    if (hit.transform == damageable.transform)
                    {
                        //go.ApplyDamage(m_GunStats.damage);
                        damageable.ApplyDamage(m_GunStats.damageableData);
                        if (m_Gun.GunID != EGun.Shotgun && m_Gun.GunID != EGun.SuperShotgun)
                        {
                            break;
                        }
                    }
                }
            }
        }

        m_GunStats.totalAmmos--;
        m_BulletsShot++;

        if(m_AllowInvoke)
        {
            Invoke("ResetShot", m_GunStats.timeBetweenShooting);
            m_AllowInvoke = false;
        }

        if (m_BulletsShot < m_GunStats.bulletsPerTap && m_GunStats.totalAmmos > 0)
        {
            Invoke("Shoot", m_GunStats.timeBetweenShots);
        }
    }

    private void ResetShot()
    {
        m_ReadyToShoot = true;
        m_AllowInvoke = true;
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        SFX_Audio fireSound = PoolMgr.Instance.Spawn("SFX_Audio", transform.position, Quaternion.identity).GetComponent<SFX_Audio>();
        fireSound.SetUp(clip);
        fireSound.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (0 != (LayerMask.GetMask("Enemy") & 1 << other.gameObject.layer))
        {
            Damageable damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                if (!damageable.IsDead)
                    m_Enemies.Add(damageable);
            }
            //m_Enemies.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (0 != (LayerMask.GetMask("Enemy") & 1 << other.gameObject.layer))
        {
            Damageable damageable = other.GetComponent<Damageable>();
            if (damageable != null)
                m_Enemies.Remove(damageable);
            //m_Enemies.Remove(other.gameObject);
        }
    }
}