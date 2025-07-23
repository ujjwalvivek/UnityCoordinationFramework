using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VisualsUI : MonoBehaviour
{
    [Header("Selection Visual Element")]
    public GameObject[] selection;

    [Header("Weapon Name")]
    public TMP_Text weaponName;


    private FirstPersonController fpsController;

    public void ShowElement(int _index)
    {
        selection[_index].SetActive(true);
    }

    public void HideElement(int _index)
    {
        selection[_index].SetActive(false);
    }
}
