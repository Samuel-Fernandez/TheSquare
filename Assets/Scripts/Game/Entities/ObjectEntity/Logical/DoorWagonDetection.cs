using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorWagonDetection : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<WagonBehavior>() && !GetComponent<DoorBehiavor>().isOpen)
        {
            GetComponent<DoorBehiavor>().OpenDoor();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<WagonBehavior>() && GetComponent<DoorBehiavor>().isOpen)
        {
            GetComponent<DoorBehiavor>().CloseDoor();
        }
    }
}
