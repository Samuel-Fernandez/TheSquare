using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestSelection : MonoBehaviour
{
    public Quests quest;
    public Image image;
    public TextMeshProUGUI questTitle;

    public void SetQuestSelection(Quests quest)
    {
        this.quest = quest;
        this.questTitle.text = LocalizationManager.instance.GetText("QUEST", quest.id + "_NAME");
    }

    public void Selection()
    {
        QuestManager.instance.ResetAllSelector();
        this.image.color = new Color(0.24f, 0.74f, 0.19f);

        QuestManager.instance.SelectQuest(quest);
    }

    public void ResetColor()
    {
        this.image.color = new Color(0.3f, 0.3f, 0.3f);
    }


}
