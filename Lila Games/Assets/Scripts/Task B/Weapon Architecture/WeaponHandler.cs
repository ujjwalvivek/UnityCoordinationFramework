using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    #region Weapon Manager

    public Item[] items;
    [HideInInspector] public int itemIndex;
    [HideInInspector] public int previousItemIndex = -1;

    #endregion

    public void Start()
    {
        EquipItems(0);
    }

    public void Update()
    {
        SwitchWeapon();
    }

    public void EquipItems(int _index)
    {
        if (_index == previousItemIndex)
            return;

        itemIndex = _index;
        Debug.Log("Enabled " + items[itemIndex].weaponGameObject.name);
        items[itemIndex].weaponGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            Debug.Log("Disabled " + items[previousItemIndex].weaponGameObject.name);
            items[previousItemIndex].weaponGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;
    }

    public void SwitchWeapon()
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex <= 0)
            {
                EquipItems(items.Length - 1);
            }
            else
            {
                EquipItems(itemIndex - 1);
            }
        }

        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItems(0);
            }
            else
            {
                EquipItems(itemIndex + 1);
            }
        }
    }
}
