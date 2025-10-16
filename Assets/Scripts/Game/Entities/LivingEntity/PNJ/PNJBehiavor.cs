using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum PNJType
{
    NONE,
    NORMAL,
    TRADER,
    EVENTER,
    QUESTER
}

public enum TraderType
{
    NONE,
    ARMORER,
    EXPLORER,
    HEALER,
}

public class PNJBehiavor : MonoBehaviour
{
    [SerializeField] public PNJMovement movement;
    private Rigidbody2D rb2d;
    private ObjectAnimation objectAnimation;
    private Vector2 lastDirection;
    public string id;
    public PNJType type;
    public TraderType traderType;
    public bool reversedSprite = false;

    public float littleBubbleUpPosition = 0;

    public TraderInventory traderInventory;

    const string littleBubble = "_LITTLE_BUBBLE";
    const string bubble = "_BUBBLE";
    List<string> speakingTexts;
    bool wait;

    //Quests
    public GameObject questExclamationPrefab; // Point d'exclamation si quête disponible
    GameObject exclamation = null; // GameObject qui se fait instancier
    public Quests quest;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        objectAnimation = GetComponent<ObjectAnimation>();

        lastDirection = Vector2.zero;

        if (movement != null)
        {
            StartCoroutine(RoutineMovement());
        }
        speakingTexts = LocalizationManager.instance.GetTexts("pnj_text", id + littleBubble);
        StartCoroutine(RoutineSpeaking());
    }

    private void Update()
    {
        QuestGestion();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.GetComponent<Stats>() && collision.GetComponent<Stats>().entityType == EntityType.Player && PlayerManager.instance.playerInputActions.Gameplay.Interaction.triggered && !wait)
        {
            if (type == PNJType.NORMAL)
            {
                NotificationManager.instance.ShowBubble(LocalizationManager.instance.GetTexts("pnj_text", id + bubble));
                StatsManager.instance.PNJSpoken(this.id);
            }
            else if(type == PNJType.TRADER)
            {
                MarketManager.instance.ToggleMarket(traderInventory);
            }
            else if(type == PNJType.EVENTER)
            {

            }
            else if (type == PNJType.QUESTER)
            {
                StatsManager.instance.PNJSpoken(this.id);

                if (!QuestManager.instance.IsInFinished(quest))
                {
                    // GET QUEST
                    if (!QuestManager.instance.IsInWaiting(quest))
                    {
                        NotificationManager.instance.ShowBubble(
                            LocalizationManager.instance.GetTexts("pnj_text", id + bubble + "_START_QUEST"),
                            () => {
                                Quests questInstance = ScriptableObjectUtility.Clone(quest);
                                questInstance.pnjID = this.id;
                                QuestManager.instance.OpenAcceptUI(questInstance);
                            }
                        );
                    }
                    // FINISH QUEST
                    else if (QuestManager.instance.IsInWaiting(quest) && QuestManager.instance.ObjectivesDone(quest))
                    {
                        if (quest.reward != QuestReward.EQUIPEMENT || (quest.reward == QuestReward.EQUIPEMENT && Equipement.instance.CheckInventory(quest.rewardEquipement.Count)))
                        {
                            NotificationManager.instance.ShowBubble(
                            LocalizationManager.instance.GetTexts("pnj_text", id + bubble + "_FINISH_QUEST"),
                            () => {
                                QuestManager.instance.FinishQuest(quest);
                            }
                        );
                        }
                        else if((quest.reward == QuestReward.EQUIPEMENT && !Equipement.instance.CheckInventory(quest.rewardEquipement.Count)))
                        {
                            NotificationManager.instance.ShowBubble(LocalizationManager.instance.GetTexts("pnj_text", "INVENTORY_FULL"));
                        }
                        
                    }
                    else if (QuestManager.instance.IsInWaiting(quest) && !QuestManager.instance.ObjectivesDone(quest))
                    {
                        NotificationManager.instance.ShowBubble(
                            LocalizationManager.instance.GetTexts("pnj_text", id + bubble + "_WAITING_QUEST")
                        );
                    }
                }
                else
                {
                    NotificationManager.instance.ShowBubble(
                        LocalizationManager.instance.GetTexts("pnj_text", id + bubble + "_FINISHED_QUEST")
                    );
                }
            }





            StartCoroutine(WaitRoutine());
        }

    }

    public void QuestGestion()
    {
        if (type != PNJType.QUESTER) return;

        bool requirementDone = QuestManager.instance.RequirementDone(quest);
        bool isInWaiting = QuestManager.instance.IsInWaiting(quest);
        bool objectivesDone = QuestManager.instance.ObjectivesDone(quest);
        bool isInFinished = QuestManager.instance.IsInFinished(quest);

        // Si la quête est terminée ou l'exclamation doit être retirée
        if (!requirementDone || isInFinished)
        {
            if (exclamation)
            {
                Destroy(exclamation);
                exclamation = null;
            }
            return; // Sortir après avoir détruit l'exclamation si la condition n'est pas remplie
        }

        // Si les conditions de la quête sont remplies
        if (!isInWaiting || (isInWaiting && objectivesDone))
        {
            if (!exclamation)
            {
                exclamation = Instantiate(questExclamationPrefab, new Vector3(this.transform.position.x, this.transform.position.y + .75f, 0), Quaternion.identity);
            }
        }
        else
        {
            if (exclamation)
            {
                Destroy(exclamation);
                exclamation = null;
            }
        }
    }


    IEnumerator WaitRoutine()
    {
        wait = true;
        yield return new WaitForSeconds(.5f);
        wait = false;
    }

    IEnumerator RoutineSpeaking()
    {
        while (true && speakingTexts != null && speakingTexts.Count > 0)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 20f));  // Préciser UnityEngine.Random
            NotificationManager.instance.ShowLittleBubble(gameObject, speakingTexts[UnityEngine.Random.Range(0, speakingTexts.Count)], 5, littleBubbleUpPosition);
        }

        yield return null;
    }

    IEnumerator RoutineMovement()
    {
        for (int i = 0; i < movement.movements.Count; i++)
        {
            PNJMovements currentMovement = movement.movements[i];

            foreach (Vector2 relativeTarget in currentMovement.movement)
            {
                Vector2 startPos = transform.position;
                Vector2 targetPos = startPos + relativeTarget;

                yield return StartCoroutine(MoveOverTimeWithAnimation(targetPos, currentMovement.duration));
            }

            if (currentMovement.isWaiting)
            {
                PlayIdleAnimation();
                yield return new WaitForSeconds(currentMovement.duration);
            }
        }

        // Quand tous les mouvements sont finis -> PNJ idle
        PlayIdleAnimation();
    }


    private IEnumerator MoveOverTimeWithAnimation(Vector2 targetPos, float duration)
    {
        float elapsedTime = 0f;
        Vector2 previousPos = transform.position; // position de la frame précédente
        Vector2 startPos = previousPos;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Vector2 newPosition = Vector2.Lerp(startPos, targetPos, t);

            // Déplacer le PNJ à la nouvelle position
            transform.position = newPosition;

            // Calculer le déplacement réel entre cette frame et la précédente
            Vector2 delta = newPosition - previousPos;

            if (delta.magnitude > 0.001f) // seuil pour éviter un idle trop tôt
                PlayMovementAnimation(delta.normalized);
            else
                PlayIdleAnimation();

            previousPos = newPosition; // maj pour la prochaine frame
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Assurer que le PNJ est exactement à la position cible
        transform.position = targetPos;
        PlayIdleAnimation();
    }


    private void PlayMovementAnimation(Vector2 direction)
    {
        // Déterminer l'animation en fonction de la direction
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Mouvement horizontal
            objectAnimation.PlayAnimation("Side");

            // Appliquer le flip du sprite
            FlipSprite(direction.x > 0);
        }
        else
        {
            // Mouvement vertical
            if (direction.y > 0)
                objectAnimation.PlayAnimation("Up");
            else
                objectAnimation.PlayAnimation("Down");

            // Ne pas flipper le sprite pour les mouvements verticaux
            FlipSprite(false);
        }

        // Mettre à jour la dernière direction
        lastDirection = direction;
    }

    private void PlayIdleAnimation()
    {
        // Jouer l'animation d'attente en fonction de la dernière direction
        if (Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
        {
            // Dernière direction horizontale
            objectAnimation.PlayAnimation("AfkSide");
            FlipSprite(lastDirection.x > 0);
        }
        else
        {
            // Dernière direction verticale
            if (lastDirection.y > 0)
                objectAnimation.PlayAnimation("AfkUp");
            else
                objectAnimation.PlayAnimation("AfkDown");

            // Ne pas flipper le sprite pour les animations verticales
            FlipSprite(false);
        }
    }

    private void FlipSprite(bool flip)
    {
        Vector3 scale = transform.localScale;
        scale.x = (flip ^ reversedSprite) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x); // Inverser seulement la valeur de l'échelle x
        transform.localScale = scale;
    }
}
