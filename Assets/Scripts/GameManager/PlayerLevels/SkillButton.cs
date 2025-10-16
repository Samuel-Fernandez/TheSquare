using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public Skill skill;
    public Image image;
    public Button button;

    private void Start()
    {
        image.sprite = skill.img;
        UpdateButton();
    }


    public void BuySkill()
    {
        PlayerLevels.instance.SkillButtonClick(gameObject);
        Select();
    }

    public void Unselect()
    {
        UpdateButton();
    }

    public void Select()
    {
        button.GetComponent<Image>().color = selectedColor;
    }

    Color canBeBoughtColor = new Color(161 / 255f, 204 / 255f, 155 / 255f); 
    Color cantBeBoughtColor = new Color(18 / 255f, 18 / 255f, 18 / 255f); 
    Color alreadyBoughtColor = new Color(27 / 255f, 184 / 255f, 6 / 255f);
    Color selectedColor = new Color(104 / 255f, 102 / 255f, 222 / 255f);

    public void UpdateButton()
    {
        // Vérifier si la compétence est déjà achetée
        if (PlayerLevels.instance.acquiredSkillsID.Contains(skill.id))
        {
            // Compétence déjà achetée
            button.enabled = true;
            button.GetComponent<Image>().color = alreadyBoughtColor;
            //image.color = alreadyBoughtColor;
            return;
        }

        bool allSkillsAcquired = true;

        // Vérifier si des compétences précédentes sont requises
        if (skill.previousSkills != null)
        {
            // Pour chaque compétence requise
            foreach (Skill requiredSkill in skill.previousSkills)
            {
                // Vérifier si la compétence requise est acquise
                if (!PlayerLevels.instance.acquiredSkillsID.Contains(requiredSkill.id))
                {
                    allSkillsAcquired = false;
                    break; // Arrêter la boucle si une compétence requise n'est pas trouvée
                }
            }
        }

        // Mise à jour de l'état et de l'apparence du bouton en fonction des compétences requises
        if (allSkillsAcquired)
        {
            button.enabled = true; // Le bouton est activé
            button.GetComponent<Image>().color = canBeBoughtColor;
            //image.color = canBeBoughtColor;
        }
        else
        {
            button.enabled = false; // Le bouton est désactivé
            button.GetComponent<Image>().color = cantBeBoughtColor;
            //image.color = cantBeBoughtColor;
        }
    }


    public bool AlreadyBought()
    {
        foreach (string id in PlayerLevels.instance.acquiredSkillsID)
        {
            if (id == skill.id)
            {
                button.GetComponent<Image>().color = alreadyBoughtColor;
                return true;
            }
        }

        return false;
    }
}
