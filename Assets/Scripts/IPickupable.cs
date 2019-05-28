using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickupable
{
    bool CanPickUp { get; set; }
    Item PickUp();
}
