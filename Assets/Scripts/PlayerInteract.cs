using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float pickUpRange = 2f;

    List<Item> inventory = new List<Item>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FindNearbyItems();
        }
    }

    void FindNearbyItems()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, pickUpRange);

        for (int i = 0; i < nearbyObjects.Length; i++)
        {
            IInteractable objectIInteract = nearbyObjects[i].GetComponent<IInteractable>();
            if (objectIInteract != null)
            {
                for (int j = 0; j < inventory.Count; j++)
                {
                    bool success = objectIInteract.Interact(inventory[j]);
                    if (success)
                    {
                        inventory.RemoveAt(j);
                        break;
                    }
                }
            }

            IPickupable objectIPickupable = nearbyObjects[i].GetComponent<IPickupable>();
            if (objectIPickupable != null)
            {
                Item pickupItem = objectIPickupable.PickUp();
                if (pickupItem != null)
                {
                    inventory.Add(pickupItem);
                }
            }
        }
    }
}
