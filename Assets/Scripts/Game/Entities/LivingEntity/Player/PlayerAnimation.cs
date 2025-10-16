using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    PlayerController controller;
    ObjectAnimation objectAnimation;
    public bool off;

    int lastMove = 0;

    public int GetLastMove()
    {
        return lastMove;
    }


    private void OnDisable()
    {
        GetComponent<ObjectAnimation>().StopAllAnimations();
    }

    void Start()
    {
        controller = GetComponent<PlayerController>();
        objectAnimation = GetComponent<ObjectAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!off)
        {
            if (GetComponent<Stats>().isDying)
            {
                GetComponent<ObjectAnimation>().PlayAnimation("Die");
            }
            else if (GetComponent<UseSpecialObject>().isShielding)
            {
                switch (lastMove)
                {
                    case 1:
                        objectAnimation.PlayAnimation("ShieldUp", true);
                        break;
                    case 2:
                        objectAnimation.PlayAnimation("ShieldDown", true);
                        break;
                    case 3:
                        objectAnimation.PlayAnimation("ShieldSide", true);
                        break;
                    default:
                        objectAnimation.PlayAnimation("ShieldSide", true);
                        break;
                }
            }
            else if (GetComponent<UseSpecialObject>().isLightning)
            {
                if (!GetComponent<UseSpecialObject>().lanternIsOn)
                {
                    switch (lastMove)
                    {
                        case 1:
                            objectAnimation.PlayAnimation("LanternOnUp", false);
                            break;
                        case 2:
                            objectAnimation.PlayAnimation("LanternOnDown", false);
                            break;
                        case 3:
                            objectAnimation.PlayAnimation("LanternOnSide", false);
                            break;
                        default:
                            objectAnimation.PlayAnimation("LanternOnSide", false);
                            break;
                    }
                }
                else
                {
                    switch (lastMove)
                    {
                        case 1:
                            objectAnimation.PlayAnimation("LanternOffUp", false);
                            break;
                        case 2:
                            objectAnimation.PlayAnimation("LanternOffDown", false);
                            break;
                        case 3:
                            objectAnimation.PlayAnimation("LanternOffSide", false);
                            break;
                        default:
                            objectAnimation.PlayAnimation("LanternOffSide", false);
                            break;
                    }
                }
            }
            else if (GetComponent<UseSpecialObject>().isPickaxing)
            {
                switch (lastMove)
                {
                    case 1:
                        objectAnimation.PlayAnimation("PickaxeUp", false, false, 1 + PlayerManager.instance.pickaxeSpeed);
                        break;
                    case 2:
                        objectAnimation.PlayAnimation("PickaxeDown", false, false, 1 + PlayerManager.instance.pickaxeSpeed);
                        break;
                    case 3:
                        objectAnimation.PlayAnimation("PickaxeSide", false, false, 1 + PlayerManager.instance.pickaxeSpeed);
                        break;
                    default:
                        objectAnimation.PlayAnimation("PickaxeDown", false, false, 1 + PlayerManager.instance.pickaxeSpeed);
                        break;
                }
            }
            else if (GetComponent<Stats>().isBowShooting)
            {
                switch (lastMove)
                {
                    case 1:
                        objectAnimation.PlayAnimation("BowUp", true, false, 1 + PlayerManager.instance.bowSpeed);
                        break;
                    case 2:
                        objectAnimation.PlayAnimation("BowDown", true, false, 1 + PlayerManager.instance.bowSpeed);
                        break;
                    case 3:
                        objectAnimation.PlayAnimation("BowSide", true, false, 1 + PlayerManager.instance.bowSpeed);
                        break;
                    default:
                        objectAnimation.PlayAnimation("BowDown", true, false, 1 + PlayerManager.instance.bowSpeed);
                        break;
                }
            }
            else if (GetComponent<UseSpecialObject>().isHammering)
            {
                switch (lastMove)
                {
                    case 1:
                        objectAnimation.PlayAnimation("HammerUp", false);
                        break;
                    case 2:
                        objectAnimation.PlayAnimation("HammerDown", false);
                        break;
                    case 3:
                        objectAnimation.PlayAnimation("HammerSide", false);
                        break;
                    default:
                        objectAnimation.PlayAnimation("HammerDown", false);
                        break;
                }
            }
            else if (GetComponent<Stats>().canMove)
            {
                if (controller.isDodging)
                {
                    objectAnimation.PlayAnimation("Dodge");
                }
                else if (controller.isAttacking)
                {
                    // Jouer l'animation d'attaque en fonction du dernier mouvement du joueur
                    switch (lastMove)
                    {
                        case 1:
                            objectAnimation.PlayAnimation("AttackUp");
                            break;
                        case 2:
                            objectAnimation.PlayAnimation("AttackDown");
                            break;
                        case 3:
                            objectAnimation.PlayAnimation("AttackSide");
                            break;
                        default:
                            objectAnimation.PlayAnimation("AttackDown");
                            break;
                    }
                }
                else if (Mathf.Abs(controller.horizontalInput) > Mathf.Abs(controller.verticalInput))
                {
                    GetComponentInChildren<SpriteRenderer>().flipX = controller.horizontalInput > 0;
                    objectAnimation.PlayAnimation("MoveSide");

                    lastMove = 3; // Déplacement latéral
                }
                else if (controller.verticalInput > 0)
                {
                    objectAnimation.PlayAnimation("MoveUp");

                    lastMove = 1; // Déplacement vers le haut
                }
                else if (controller.verticalInput < 0)
                {
                    objectAnimation.PlayAnimation("MoveDown");

                    lastMove = 2; // Déplacement vers le bas
                }
                else
                {
                    // Si le joueur ne se déplace pas et n'attaque pas, jouer l'animation afk appropriée
                    switch (lastMove)
                    {
                        case 1:
                            objectAnimation.PlayAnimation("AfkUp");
                            break;
                        case 2:
                            objectAnimation.PlayAnimation("AfkDown");
                            break;
                        case 3:
                            objectAnimation.PlayAnimation("AfkSide");
                            break;
                        default:
                            objectAnimation.PlayAnimation("AfkDown");
                            break;
                    }
                }
            }
            else
            {
                // Si le joueur ne se déplace pas et n'attaque pas, jouer l'animation afk appropriée
                switch (lastMove)
                {
                    case 1:
                        objectAnimation.PlayAnimation("AfkUp");
                        break;
                    case 2:
                        objectAnimation.PlayAnimation("AfkDown");
                        break;
                    case 3:
                        objectAnimation.PlayAnimation("AfkSide");
                        break;
                    default:
                        objectAnimation.PlayAnimation("AfkDown");
                        break;
                }
            }
        }
        
        
    }


}
