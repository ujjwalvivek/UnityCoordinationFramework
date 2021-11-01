using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public abstract class Item : MonoBehaviour
{
    [Header("Weapon References")]
    public WeaponData weaponData;
    public GameObject weaponGameObject;

    public abstract void UseWeapon();
}
