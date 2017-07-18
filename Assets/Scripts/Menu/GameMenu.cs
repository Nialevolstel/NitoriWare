﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Attach to every menu animator
public class GameMenu : MonoBehaviour
{
    public static SubMenu subMenu = SubMenu.Splash;
    //public static SubMenu subMenu = SubMenu.Title;  //Debug purposes
    public static bool shifting;

    private static GameMenu shiftOrigin;

    public enum SubMenu
    {
        Splash = 0,
        Title = 1,
        Settings = 2
    }

    void Awake()
    {
        shifting = (subMenu == SubMenu.Splash);
        setSubMenu((int)subMenu);

        MenuAnimationUpdater updater = GetComponent<MenuAnimationUpdater>();
        if (updater != null)
            updater.updateAnimatorValues();
    }

    public void shift(int subMenu)
    {
        setSubMenu(subMenu);
        shifting = true;
        shiftOrigin = this;
    }

    public void endShift()
    {
        if (shiftOrigin != this)
            shifting = false;
    }

    void setSubMenu(int subMenu)
    {
        GameMenu.subMenu = (SubMenu)subMenu;
    }

    void setShifting(bool shifting)
    {
        GameMenu.shifting = shifting;
    }
}