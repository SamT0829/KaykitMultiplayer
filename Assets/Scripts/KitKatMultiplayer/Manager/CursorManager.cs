using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour
{
    public RectTransform hand;
    private Vector3 mouseWorldPos => Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));
    private bool canClick;
    private bool holdItem;

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void Update()
    {
        canClick = ObjectAtMousePosition();

        if (hand.gameObject.activeInHierarchy)
        {
            hand.position = Input.mousePosition;
        }
        if (IntractWithUI()) return;

        if (canClick && Input.GetMouseButtonDown(0))
        {
            ClickAction(ObjectAtMousePosition().gameObject);
        }
    }


    private void ClickAction(GameObject clickObject)
    {
        switch (clickObject.tag)
        {
            case "Teleport":
                //         Teleport teleport = clickObject.GetComponent<Teleport>();
                //         teleport?.TeleportToScene();
                break;
                //     case "Item":
                //         Item item = clickObject.GetComponent<Item>();
                //         item?.ItemClicked();
                //         break;
                //     case "Interactive":
                //         Interactive interactive = clickObject.GetComponent<Interactive>();
                //         if (holdItem)
                //             interactive?.CheckItem(currenItem);
                //         else
                //             interactive?.EmptyClicked();
                //         break;
        }
    }

    /// <summary>
    /// 檢測鼠標點擊範圍的碰撞體
    /// </summary>
    /// <returns></returns>
    private Collider2D ObjectAtMousePosition()
    {
        return Physics2D.OverlapPoint(mouseWorldPos);
    }

    private bool IntractWithUI()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;

        return false;
    }
}
