using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "First Person Shooter Game/New Weapon")]
public class WeaponVariables : WeaponData
{
    [Header("AutoFire / Single Shot")]
    public bool allowButtonHold;

    [Header("Weapon Stats")]
    public int damage; // Unused Variable
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, numberOfMags, totalAmmoCount, bulletsPerTap;
    [HideInInspector] public int bulletsLeft;
    [HideInInspector] public int bulletsShot;

    [Header("Visual Effects")]
    public ParticleSystem bulletImpact;
    public ParticleSystem muzzleFlash;

    [Header("Screen Shake")]
    public float camShakeMagnitude;
    public float camShakeDuration;

    [Header("Shooting Projectiles")]
    public float shootForce;
    public float upwardForce;
    public GameObject bulletPrefab;

    [Header("Reloading Identifier")]
    public GameObject reloadingIdentifier;

    //Internal Variables
    [HideInInspector] public bool shooting;
    [HideInInspector] public bool readyToShoot;
    [HideInInspector] public bool reloading;

}
