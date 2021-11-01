using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShotWeapons : Weapons
{
    [Header("References")]
    [SerializeField] Camera mainCam;
    public CamShake camShake;
    public GameObject attackPoint;
    public GameObject reloadTip;

    public void Awake()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            camShake = mainCam.GetComponent<CamShake>();
        }

        ((WeaponVariables)weaponData).bulletsLeft = ((WeaponVariables)weaponData).magazineSize;
        ((WeaponVariables)weaponData).readyToShoot = true;

        //((WeaponVariables)weaponData).totalAmmoCount = ((WeaponVariables)weaponData).magazineSize * ((WeaponVariables)weaponData).numberOfMags;
    }

    public void MyInput()
    {
        if (((WeaponVariables)weaponData).allowButtonHold)
        {
            ((WeaponVariables)weaponData).shooting = Input.GetKey(KeyCode.Mouse0);
        }

        else
        {
            ((WeaponVariables)weaponData).shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }  
    }

    public override void UseWeapon()
    {
        MyInput();

        if (((WeaponVariables)weaponData).readyToShoot && ((WeaponVariables)weaponData).shooting && !((WeaponVariables)weaponData).reloading && ((WeaponVariables)weaponData).bulletsLeft > 0 && ((WeaponVariables)weaponData).totalAmmoCount > 0)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && ((WeaponVariables)weaponData).bulletsLeft < ((WeaponVariables)weaponData).magazineSize && !((WeaponVariables)weaponData).reloading && ((WeaponVariables)weaponData).totalAmmoCount >= ((WeaponVariables)weaponData).magazineSize)
        {
            Reload();
        }

        if (((WeaponVariables)weaponData).readyToShoot && ((WeaponVariables)weaponData).shooting && !((WeaponVariables)weaponData).reloading && ((WeaponVariables)weaponData).bulletsLeft <= 0 && ((WeaponVariables)weaponData).totalAmmoCount >= ((WeaponVariables)weaponData).magazineSize)
        {
            Reload();
        }
    }

    public void Shoot()
    {
        ((WeaponVariables)weaponData).readyToShoot = false;

        float x = Random.Range(-((WeaponVariables)weaponData).spread, ((WeaponVariables)weaponData).spread);
        float y = Random.Range(-((WeaponVariables)weaponData).spread, ((WeaponVariables)weaponData).spread);

        Vector3 spreadDirection = mainCam.transform.forward + new Vector3(x, y, 0f);

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = mainCam.transform.position;
        ray.direction = spreadDirection;
        //Debug.DrawLine(ray.origin, ray.direction * ((WeaponVariables)weaponData).range, Color.red);

        if(Physics.Raycast(ray, out RaycastHit hitPoint, ((WeaponVariables)weaponData).range))
        {
            Debug.Log("Hit Point: " + hitPoint.collider.gameObject.name);
            MuzzleFlash(attackPoint.transform);
            BulletImpact(hitPoint.point, hitPoint.normal);
        }

        StartCoroutine(camShake.Shake(((WeaponVariables)weaponData).camShakeDuration, ((WeaponVariables)weaponData).camShakeMagnitude));

        ((WeaponVariables)weaponData).bulletsLeft--;
        ((WeaponVariables)weaponData).bulletsShot--;
        ((WeaponVariables)weaponData).totalAmmoCount--;

        Invoke("ResetShot", ((WeaponVariables)weaponData).timeBetweenShooting);

        if (((WeaponVariables)weaponData).bulletsShot > 0 && ((WeaponVariables)weaponData).bulletsLeft > 0)
        {
            ((WeaponVariables)weaponData).bulletsShot = ((WeaponVariables)weaponData).bulletsPerTap;
            Invoke("Shoot", ((WeaponVariables)weaponData).timeBetweenShots);
        }

    }

    public void ResetShot()
    {
        ((WeaponVariables)weaponData).readyToShoot = true;
    }

    public void Reload()
    {
        ((WeaponVariables)weaponData).reloading = true;

        StartCoroutine(ShowText());

        Invoke("ReloadEnd", ((WeaponVariables)weaponData).reloadTime);
    }

    IEnumerator ShowText()
    {
        GameObject tempReload = Instantiate(((WeaponVariables)weaponData).reloadingIdentifier, new Vector3(25f, 0f, 0f), Quaternion.identity, reloadTip.transform);
        yield return new WaitForSeconds(((WeaponVariables)weaponData).reloadTime);
        Destroy(tempReload);
    }

    public void ReloadEnd()
    {
        if (((WeaponVariables)weaponData).totalAmmoCount >= ((WeaponVariables)weaponData).magazineSize)
        {
            ((WeaponVariables)weaponData).bulletsLeft = ((WeaponVariables)weaponData).magazineSize;
        }
        else
        {
            ((WeaponVariables)weaponData).bulletsLeft = ((WeaponVariables)weaponData).totalAmmoCount;
        }
        
        ((WeaponVariables)weaponData).reloading = false;
    }

    public void BulletImpact(Vector3 hitPosition, Vector3 hitNormal)
    {
        ParticleSystem tempImpact = Instantiate(((WeaponVariables)weaponData).bulletImpact, hitPosition, Quaternion.LookRotation(hitNormal, Vector3.up) * ((WeaponVariables)weaponData).bulletImpact.transform.rotation);
        tempImpact.Play();
        ParticleSystem.MainModule _main = tempImpact.main;
        _main.stopAction = ParticleSystemStopAction.Destroy;
    }

    public void MuzzleFlash(Transform attackPoint)
    {
        ParticleSystem tempFlash = Instantiate(((WeaponVariables)weaponData).muzzleFlash, attackPoint.transform.position, attackPoint.transform.rotation);
        tempFlash.Play();
        ParticleSystem.MainModule _main = tempFlash.main;
        _main.stopAction = ParticleSystemStopAction.Destroy;
    }
}
