using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    bool CanInteract { get; set; }
    bool Interact(Item itemToInteractWith);
}
