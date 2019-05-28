using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyObject : MonoBehaviour, IPickupable
{
    public Item key;

    public bool CanPickUp { get; set; } = true;

    public Item PickUp()
    {
        if (CanPickUp)
        {
            Destroy(gameObject);
            return key;
        }
        return null;
    }
}
