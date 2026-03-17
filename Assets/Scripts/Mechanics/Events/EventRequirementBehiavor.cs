using System.Collections;
using UnityEngine;

public class EventRequirementBehiavor : MonoBehaviour
{
    public EventContainer eventRequirement;
    public GameObject cloudPrefab;
    public bool ifRequirementGoodActivate = false;
    public float initializationDelay = 0.1f; // D�lai avant la premi�re v�rification

    private bool childrenVisible = false;
    private bool hasInitialized = false;
    private bool isInitializing = false;

    public bool RequirementsGood()
    {
        if (eventRequirement)
        {
            SaveManager.instance.twoStateContainer.TryGetState(eventRequirement.ID, out bool temp);
            return temp;
        }
        else
        {
            return true;
        }
    }

    private void Start()
    {
        // D�marrer l'initialisation avec d�lai
        if (!isInitializing)
        {
            StartCoroutine(DelayedInitialization());
        }
    }

    private IEnumerator DelayedInitialization()
    {
        isInitializing = true;

        // Attendre un petit d�lai pour laisser les objets s'initialiser
        yield return new WaitForSeconds(initializationDelay);

        // Forcer la premi�re v�rification
        bool requirementOK = RequirementsGood();
        bool shouldBeActive = (requirementOK == ifRequirementGoodActivate);

        Debug.Log($"[RequirementLogic - Init] requirementOK={requirementOK}, ifRequirementGoodActivate={ifRequirementGoodActivate}, shouldBeActive={shouldBeActive}");

        ApplyStateToChildren(shouldBeActive);

        hasInitialized = true;
        isInitializing = false;
    }

    private void Update()
    {
        // Ne pas traiter pendant l'initialisation
        if (!hasInitialized || isInitializing)
            return;

        bool requirementOK = RequirementsGood();
        bool shouldBeActive = (requirementOK == ifRequirementGoodActivate);

        Debug.Log($"[RequirementLogic] requirementOK={requirementOK}, ifRequirementGoodActivate={ifRequirementGoodActivate}, shouldBeActive={shouldBeActive}");

        // V�rifier si l'�tat change
        if (childrenVisible != shouldBeActive)
        {
            ApplyStateToChildren(shouldBeActive);
        }
    }

    private void ApplyStateToChildren(bool shouldBeActive)
    {
        childrenVisible = shouldBeActive;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);
            GameObject child = childTransform.gameObject;

            Debug.Log($"Setting active state of {child.name} to {shouldBeActive}");
            child.SetActive(shouldBeActive);

            if (shouldBeActive && cloudPrefab != null)
            {
                Instantiate(cloudPrefab, childTransform.position, Quaternion.identity);
            }
        }
    }
}