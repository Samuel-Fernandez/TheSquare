using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MineralAdd : MonoBehaviour
{

    public MineralType mineralType;
    public GameObject plusButton;
    public GameObject minusButton;
    public TextMeshProUGUI nbSelected;
    public TextMeshProUGUI totalNb;
    int maxNumber;
    int actualNumber;
    int tempRemainingMineral;

    private void Update()
    {
        nbSelected.text = actualNumber.ToString();
        totalNb.text = tempRemainingMineral.ToString();

        if(AnvilUpgradeManager.instance.selectedItem)
            switch (AnvilUpgradeManager.instance.selectedItem.rarity)
            {
                case Rarity.COMMON:
                    if (actualNumber < maxNumber && AnvilUpgradeManager.instance.potentialLevel + AnvilUpgradeManager.instance.itemLevel < 3)
                        plusButton.GetComponent<Button>().interactable = true;
                    else
                        plusButton.GetComponent<Button>().interactable = false;
                    break;
                case Rarity.UNCOMMON:
                    if (actualNumber < maxNumber && AnvilUpgradeManager.instance.potentialLevel + AnvilUpgradeManager.instance.itemLevel < 5)
                        plusButton.GetComponent<Button>().interactable = true;
                    else
                        plusButton.GetComponent<Button>().interactable = false;
                    break;
                case Rarity.RARE:
                    if (actualNumber < maxNumber && AnvilUpgradeManager.instance.potentialLevel + AnvilUpgradeManager.instance.itemLevel < 7)
                        plusButton.GetComponent<Button>().interactable = true;
                    else
                        plusButton.GetComponent<Button>().interactable = false;
                    break;
                case Rarity.EPIC:
                    if (actualNumber < maxNumber && AnvilUpgradeManager.instance.potentialLevel + AnvilUpgradeManager.instance.itemLevel < 9)
                        plusButton.GetComponent<Button>().interactable = true;
                    else
                        plusButton.GetComponent<Button>().interactable = false;
                    break;
                case Rarity.LEGENDARY:
                    if (actualNumber < maxNumber && AnvilUpgradeManager.instance.potentialLevel + AnvilUpgradeManager.instance.itemLevel < 10)
                        plusButton.GetComponent<Button>().interactable = true;
                    else
                        plusButton.GetComponent<Button>().interactable = false;
                    break;
                default:
                    break;
            }

        if (actualNumber > 0)
            minusButton.GetComponent<Button>().interactable = true;
        else
            minusButton.GetComponent<Button>().interactable = false;

    }

    public void RemoveMinerals()
    {
        switch (mineralType)
        {
            case MineralType.IRON:
                PlayerManager.instance.GetSpecialItem(SpecialItemType.IRON).nb -= actualNumber;
                break;
            case MineralType.SILVER:
                PlayerManager.instance.GetSpecialItem(SpecialItemType.SILVER).nb -= actualNumber;
                break;
            case MineralType.DIAMOND:
                PlayerManager.instance.GetSpecialItem(SpecialItemType.DIAMOND).nb -= actualNumber;
                break;
            case MineralType.ANTIMATTER:
                PlayerManager.instance.GetSpecialItem(SpecialItemType.ANTIMATTER).nb -= actualNumber;
                break;
            case MineralType.SQUAREBLOCK:
                PlayerManager.instance.GetSpecialItem(SpecialItemType.SQUAREBLOCK).nb -= actualNumber;
                break;
            default:
                break;
        }
    }


    public int GetPotentialExp()
    {
        switch (mineralType)
        {
            case MineralType.IRON:
                return 1 * actualNumber;
            case MineralType.SILVER:
                return 2 * actualNumber;
            case MineralType.DIAMOND:
                return 5 * actualNumber;
            case MineralType.ANTIMATTER:
                return 10 * actualNumber;
            case MineralType.SQUAREBLOCK:
                return 20 * actualNumber;
            default:
                break;
        }

        return 0;
    }

    public void Plus()
    {
        AnvilUpgradeManager.instance.GetComponent<SoundContainer>().PlaySound("AddMineral", 2);
        if (actualNumber < maxNumber)
        {
            actualNumber++;
            tempRemainingMineral--;
        }
    }

    public void Minus()
    {
        AnvilUpgradeManager.instance.GetComponent<SoundContainer>().PlaySound("AddMineral", 2);

        if (actualNumber > 0)
        {
            actualNumber--;
            tempRemainingMineral++;
        }
    }

    public void Initialize()
    {
        actualNumber = 0;

        switch (mineralType)
        {
            case MineralType.IRON:
                gameObject.SetActive(PlayerManager.instance.GetSpecialItem(SpecialItemType.IRON).nb > 0);
                maxNumber = PlayerManager.instance.GetSpecialItem(SpecialItemType.IRON).nb;
                break;
            case MineralType.SILVER:
                gameObject.SetActive(PlayerManager.instance.GetSpecialItem(SpecialItemType.SILVER).nb > 0);
                maxNumber = PlayerManager.instance.GetSpecialItem(SpecialItemType.SILVER).nb;
                break;
            case MineralType.DIAMOND:
                gameObject.SetActive(PlayerManager.instance.GetSpecialItem(SpecialItemType.DIAMOND).nb > 0);
                maxNumber = PlayerManager.instance.GetSpecialItem(SpecialItemType.DIAMOND).nb;
                break;
            case MineralType.ANTIMATTER:
                gameObject.SetActive(PlayerManager.instance.GetSpecialItem(SpecialItemType.ANTIMATTER).nb > 0);
                maxNumber = PlayerManager.instance.GetSpecialItem(SpecialItemType.ANTIMATTER).nb;
                break;
            case MineralType.SQUAREBLOCK:
                gameObject.SetActive(PlayerManager.instance.GetSpecialItem(SpecialItemType.SQUAREBLOCK).nb > 0);
                maxNumber = PlayerManager.instance.GetSpecialItem(SpecialItemType.SQUAREBLOCK).nb;
                break;
            default:
                break;
        }

        tempRemainingMineral = maxNumber;
    }
}
