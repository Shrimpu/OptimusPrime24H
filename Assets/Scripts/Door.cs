using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public Item key;

    public bool CanInteract { get; set; } = true;

    public bool Interact(Item itemToInteractWith)
    {
        if (itemToInteractWith.Equals(key))
        {
            Open();
            return true;
        }
        return false;
    }

    void Open()
    {
        // opens door
        Destroy(gameObject);
    }
}
