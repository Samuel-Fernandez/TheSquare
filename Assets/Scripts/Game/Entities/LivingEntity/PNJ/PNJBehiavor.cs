using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // Quests - MODIFIÉ: Liste de quêtes au lieu d'une seule
    public GameObject questExclamationPrefab;
    GameObject exclamation = null;
    public List<Quests> questsList = new List<Quests>(); // NOUVEAU: Liste de quêtes

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
            else if (type == PNJType.TRADER)
            {
                MarketManager.instance.ToggleMarket(traderInventory);
            }
            else if (type == PNJType.EVENTER)
            {

            }
            else if (type == PNJType.QUESTER)
            {
                StatsManager.instance.PNJSpoken(this.id);

                // MODIFIÉ: Récupérer la quête active du PNJ
                Quests currentQuest = GetCurrentAvailableQuest();

                if (currentQuest != null)
                {
                    if (!QuestManager.instance.IsInFinished(currentQuest))
                    {
                        // GET QUEST
                        if (!QuestManager.instance.IsInWaiting(currentQuest))
                        {
                            NotificationManager.instance.ShowBubble(
                                LocalizationManager.instance.GetTexts("pnj_text", id + bubble + "_START_QUEST"),
                                () => {
                                    Quests questInstance = ScriptableObjectUtility.Clone(currentQuest);
                                    questInstance.pnjID = this.id;
                                    QuestManager.instance.OpenAcceptUI(questInstance);
                                }
                            );
                        }
                        // FINISH QUEST
                        else if (QuestManager.instance.IsInWaiting(currentQuest) && QuestManager.instance.ObjectivesDone(currentQuest))
                        {
                            if (currentQuest.reward != QuestReward.EQUIPEMENT || (currentQuest.reward == QuestReward.EQUIPEMENT && Equipement.instance.CheckInventory(currentQuest.rewardEquipement.Count)))
                            {
                                NotificationManager.instance.ShowBubble(
                                LocalizationManager.instance.GetTexts("pnj_text", id + bubble + "_FINISH_QUEST"),
                                () => {
                                    QuestManager.instance.FinishQuest(currentQuest);
                                }
                            );
                            }
                            else if ((currentQuest.reward == QuestReward.EQUIPEMENT && !Equipement.instance.CheckInventory(currentQuest.rewardEquipement.Count)))
                            {
                                NotificationManager.instance.ShowBubble(LocalizationManager.instance.GetTexts("pnj_text", "INVENTORY_FULL"));
                            }

                        }
                        else if (QuestManager.instance.IsInWaiting(currentQuest) && !QuestManager.instance.ObjectivesDone(currentQuest))
                        {
                            NotificationManager.instance.ShowBubble(
                                LocalizationManager.instance.GetTexts("pnj_text", id + bubble + "_WAITING_QUEST")
                            );
                        }
                    }
                    else
                    {
                        // Si la quête actuelle est terminée, afficher son message de fin
                        NotificationManager.instance.ShowBubble(
                            LocalizationManager.instance.GetTexts("pnj_text", id + bubble + "_FINISHED_QUEST")
                        );
                    }
                }
                else
                {
                    // Aucune quête disponible, afficher le message de la dernière quête terminée
                    Quests lastCompletedQuest = GetLastCompletedQuest();
                    if (lastCompletedQuest != null)
                    {
                        NotificationManager.instance.ShowBubble(
                            LocalizationManager.instance.GetTexts("pnj_text", id + bubble + "_FINISHED_QUEST")
                        );
                    }
                }
            }

            StartCoroutine(WaitRoutine());
        }
    }

    // NOUVEAU: Récupère la quête active disponible pour ce PNJ
    private Quests GetCurrentAvailableQuest()
    {
        if (questsList == null || questsList.Count == 0)
            return null;

        // Parcourir les quêtes dans l'ordre
        foreach (var quest in questsList)
        {
            // Si la quête n'est pas terminée et que les prérequis sont remplis
            if (!QuestManager.instance.IsInFinished(quest) && QuestManager.instance.RequirementDone(quest))
            {
                return quest;
            }
        }

        return null;
    }

    // NOUVEAU: Récupère la dernière quête terminée de ce PNJ
    private Quests GetLastCompletedQuest()
    {
        if (questsList == null || questsList.Count == 0)
            return null;

        // Parcourir les quêtes en ordre inverse pour trouver la dernière terminée
        for (int i = questsList.Count - 1; i >= 0; i--)
        {
            if (QuestManager.instance.IsInFinished(questsList[i]))
            {
                return questsList[i];
            }
        }

        return null;
    }

    public void QuestGestion()
    {
        if (type != PNJType.QUESTER) return;

        // MODIFIÉ: Utiliser la quête active
        Quests currentQuest = GetCurrentAvailableQuest();

        if (currentQuest == null)
        {
            if (exclamation)
            {
                Destroy(exclamation);
                exclamation = null;
            }
            return;
        }

        bool requirementDone = QuestManager.instance.RequirementDone(currentQuest);
        bool isInWaiting = QuestManager.instance.IsInWaiting(currentQuest);
        bool objectivesDone = QuestManager.instance.ObjectivesDone(currentQuest);
        bool isInFinished = QuestManager.instance.IsInFinished(currentQuest);

        // Si la quête est terminée ou l'exclamation doit être retirée
        if (!requirementDone || isInFinished)
        {
            if (exclamation)
            {
                Destroy(exclamation);
                exclamation = null;
            }
            return;
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
            yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 20f));
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

        PlayIdleAnimation();
    }

    private IEnumerator MoveOverTimeWithAnimation(Vector2 targetPos, float duration)
    {
        float elapsedTime = 0f;
        Vector2 previousPos = transform.position;
        Vector2 startPos = previousPos;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            Vector2 newPosition = Vector2.Lerp(startPos, targetPos, t);

            transform.position = newPosition;

            Vector2 delta = newPosition - previousPos;

            if (delta.magnitude > 0.001f)
                PlayMovementAnimation(delta.normalized);
            else
                PlayIdleAnimation();

            previousPos = newPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        PlayIdleAnimation();
    }

    private void PlayMovementAnimation(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            objectAnimation.PlayAnimation("Side");
            FlipSprite(direction.x > 0);
        }
        else
        {
            if (direction.y > 0)
                objectAnimation.PlayAnimation("Up");
            else
                objectAnimation.PlayAnimation("Down");

            FlipSprite(false);
        }

        lastDirection = direction;
    }

    private void PlayIdleAnimation()
    {
        if (Mathf.Abs(lastDirection.x) > Mathf.Abs(lastDirection.y))
        {
            objectAnimation.PlayAnimation("AfkSide");
            FlipSprite(lastDirection.x > 0);
        }
        else
        {
            if (lastDirection.y > 0)
                objectAnimation.PlayAnimation("AfkUp");
            else
                objectAnimation.PlayAnimation("AfkDown");

            FlipSprite(false);
        }
    }

    private void FlipSprite(bool flip)
    {
        Vector3 scale = transform.localScale;
        scale.x = (flip ^ reversedSprite) ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
}