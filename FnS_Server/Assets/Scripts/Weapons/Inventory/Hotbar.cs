using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hotbar : MonoBehaviour
{
    private int currentSelected = 0;

    private int oldSelected = -1;

    private static Transform[] children;

    private GunHolder gunHolder;

    private void Start()
    {
        children = gunHolder.GetAllSlots();

        Refresh();
    }

    // private void Update()
    // {
    //     SwitchWeapons();
    // }

    private void SwitchWeapons(int newSelected)
    {
        print("Hotbar" + (1+newSelected));
        currentSelected = newSelected;
        Refresh();
    }

    public void Refresh()
    {
        for(int i = 0; i < 4; ++i)
        {
            //GunHolder.Instance.SwitchShooting(i, i == currentSelected);

            children[i].gameObject.SetActive(i == currentSelected); // if index of transform == index of weapon selected
        }
    }

    public void SetGunHolder(GunHolder _gunholder) => gunHolder = _gunholder;
}
