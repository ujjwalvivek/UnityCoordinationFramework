using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HeavyWeapons : Weapons
{
    [Header("References")]
    [SerializeField] Camera mainCam;
    public CamShake camShake;
    public GameObject attackPoint;

    public GameObject bulletObject;
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
            ((WeaponVariables)weaponData).bulletsShot = 0;
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

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        RaycastHit hitPoint;

        Vector3 targetPoint;
        //Debug.DrawLine(ray.origin, ray.direction * ((WeaponVariables)weaponData).range, Color.red);

        if (Physics.Raycast(ray, out hitPoint))
        {
            targetPoint = hitPoint.point;
        }
        else
        {
            targetPoint = ray.GetPoint(200);
        }

        Vector3 directionWithoutSpread = targetPoint - attackPoint.transform.position;

        float x = Random.Range(-((WeaponVariables)weaponData).spread, ((WeaponVariables)weaponData).spread);
        float y = Random.Range(-((WeaponVariables)weaponData).spread, ((WeaponVariables)weaponData).spread);

        Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0);

        if (bulletObject.transform.childCount == 1)
        {
            bulletObject.transform.GetChild(0).transform.forward = directionWithSpread.normalized;

            if (hitPoint.collider.gameObject != null)
            {
                Destroy(bulletObject.transform.GetChild(0).gameObject);
            }
        }
        else
        {
            return;
        }

        MuzzleFlash(attackPoint.transform);
        BulletImpact(targetPoint, hitPoint.normal);

        StartCoroutine(camShake.Shake(((WeaponVariables)weaponData).camShakeDuration, ((WeaponVariables)weaponData).camShakeMagnitude));

        mainCam.transform.root.GetComponent<FirstPersonController>().Recoil(((WeaponVariables)weaponData).recoil, ((WeaponVariables)weaponData).revert);

        ((WeaponVariables)weaponData).bulletsLeft--;
        ((WeaponVariables)weaponData).bulletsShot++;
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
        StartCoroutine(GenerateBullet());

        Invoke("ReloadEnd", ((WeaponVariables)weaponData).reloadTime);
    }

    IEnumerator GenerateBullet()
    {
        yield return new WaitForSeconds(((WeaponVariables)weaponData).reloadTime);
        GameObject tempObj = Instantiate(((WeaponVariables)weaponData).bulletPrefab, bulletObject.transform.position, Quaternion.identity);
        tempObj.transform.SetParent(bulletObject.transform);
        tempObj.transform.localPosition = Vector3.zero;
        tempObj.transform.localRotation = Quaternion.Euler(Vector3.zero);
        tempObj.transform.localScale = Vector3.one;
    }

    IEnumerator ShowText()
    {
        GameObject tempReload = Instantiate(((WeaponVariables)weaponData).reloadingIdentifier, new Vector3(25f, 0f, 0f), Quaternion.identity, reloadTip.transform);
        yield return new WaitForSeconds(((WeaponVariables)weaponData).reloadTime);
        Destroy(tempReload);
    }

    public void ReloadEnd()
    {
        if (((WeaponVariables)weaponData).totalAmmoCount > ((WeaponVariables)weaponData).magazineSize)
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
