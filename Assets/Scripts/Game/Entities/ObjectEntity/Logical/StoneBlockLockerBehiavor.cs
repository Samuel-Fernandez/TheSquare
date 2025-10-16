using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneBlockLockerBehiavor : MonoBehaviour
{
    public string id;
    public GameObject uiInteract;

    GameObject instanceUiInteract;

    private void Start()
    {
        bool state;

        SaveManager.instance.twoStateContainer.TryGetState(id, out state);

        // Considéré comme ouvert dans la sauvegarde
        if (state)
        {
            Destroy(gameObject);
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        if(DungeonManager.instance.actualDungeon.nbKeys > 0)
        {
            if (collision.gameObject.GetComponent<Stats>() && collision.gameObject.GetComponent<Stats>().entityType == EntityType.Player)
            {
                // Créer l'UI d'interaction si elle n'existe pas encore
                if (instanceUiInteract == null)
                {
                    Vector3 uiPosition = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                    instanceUiInteract = Instantiate(uiInteract, uiPosition, Quaternion.identity);
                }

                if (PlayerManager.instance.playerInputActions.Gameplay.Interaction.triggered)
                {
                    SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, true);
                    Unlock();
                    DungeonManager.instance.RemoveKey();
                }
            }
        }
        
    }

    void Unlock()
    {

        GetComponent<SoundContainer>().PlaySound("Unlock", 2);
        if (instanceUiInteract)
            Destroy(instanceUiInteract);
        Destroy(gameObject);
    }
}
