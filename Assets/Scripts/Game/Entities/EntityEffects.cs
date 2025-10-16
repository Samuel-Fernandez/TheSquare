using System.Collections;
using UnityEngine;

public class EntityEffects : MonoBehaviour
{
    public GameObject fireEffect;
    public int fireForce;
    public GameObject freezeEffect;
    public GameObject poisonEffect;
    public GameObject slimeEffect;

    public int poisonForce;

    public bool stopEffects;


    public bool canBeFire;
    public bool canBeFreeze;
    public bool canBePoison;
    public bool canBeSlimed;

    public bool isFire;
    public bool isFreeze;
    public bool isPoison;
    public bool isSlimed;

    public int fireTimer;
    public int freezeTimer;
    public int poisonTimer;
    public int slimeTimer;

    private bool isFireRoutineRunning;
    private bool isFreezeRoutineRunning;
    private bool isPoisonRoutineRunning;
    private bool isSlimedRoutineRunning;

    private void Update()
    {

        DetermineState();

        if(GetComponent<Stats>() && GetComponent<Stats>().isDying)
        {
            ResetEffect();
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    // Permet d'inverser la possibilité d'être sensible à un effet
    public void ToggleAffectations()
    {
        canBeFire = !canBeFire;
        canBeFreeze = !canBeFreeze;
        canBePoison = !canBePoison;
        canBeSlimed = !canBeSlimed;
    }

    public void SetState(int force = 0, bool isFire = false, bool isFreeze = false, bool isPoison = false, bool isSlimed = false)
    {
        if (!GetComponent<Stats>() || (GetComponent<Stats>() && !GetComponent<Stats>().isDying && !stopEffects))
        {
            if (canBeFire)
            {
                this.isFire = isFire ? isFire : this.isFire;

                if (isFire)
                    fireForce = Mathf.Max(force, 1);
            }
            
            if (canBeFreeze)
            {

                this.isFreeze = isFreeze ? isFreeze : this.isFreeze;
            }
            
            if (canBePoison && GetComponent<LifeManager>())
            {
                this.isPoison = isPoison ? isPoison : this.isPoison;

                if (isPoison)
                    poisonForce = force;
            }

            if(canBeSlimed)
            {
                this.isSlimed = isSlimed ? isSlimed : this.isSlimed;
            }

            DetermineState();
        }

    }

    void DetermineState()
    {
        fireEffect.SetActive(isFire);
        freezeEffect.SetActive(isFreeze);
        poisonEffect.SetActive(isPoison);
        slimeEffect.SetActive(isSlimed);

        if(isSlimed)
        {
            if (!isSlimedRoutineRunning)
            {
                slimeEffect.GetComponent<ObjectAnimation>().PlayAnimation("Slime");
                slimeTimer = Random.Range(Mathf.RoundToInt(5 - (5 * PlayerManager.instance.negativeEffectReducer)), Mathf.RoundToInt(8 - (8 * PlayerManager.instance.negativeEffectReducer))); // Adjusted timer range for example
                StartCoroutine(OnSlimeRoutine());
                isSlimedRoutineRunning = true; // Ajout : marquer que la routine est en cours
            }
        }
        else
        {
            isSlimedRoutineRunning = false;
        }

        if (isFire)
        {
            if (!isFireRoutineRunning)
            {
                fireEffect.GetComponent<ObjectAnimation>().PlayAnimation("Fire");
                fireTimer = Random.Range(Mathf.RoundToInt(5 - (5 * PlayerManager.instance.negativeEffectReducer)), Mathf.RoundToInt(10 - (10 * PlayerManager.instance.negativeEffectReducer))); // Adjusted timer range for example
                StartCoroutine(OnFireRoutine());
                isFireRoutineRunning = true; // Ajout : marquer que la routine est en cours
            }
        }
        else
        {
            // Ajout : Réinitialisation si l'effet n'est plus actif
            isFireRoutineRunning = false;
        }

        if (isFreeze)
        {
            if (!isFreezeRoutineRunning)
            {
                freezeEffect.GetComponent<ObjectAnimation>().PlayAnimation("Freeze");
                freezeEffect.GetComponent<SoundContainer>().PlaySound("Freeze", 2);
                freezeTimer = Random.Range(Mathf.RoundToInt(3 - (3 * PlayerManager.instance.negativeEffectReducer)), Mathf.RoundToInt(6 - (6 * PlayerManager.instance.negativeEffectReducer))); // Adjusted timer range for example
                StartCoroutine(OnFreezeRoutine());
                isFreezeRoutineRunning = true; // Ajout : marquer que la routine est en cours
            }
        }
        else
        {
            // Ajout : Réinitialisation si l'effet n'est plus actif
            isFreezeRoutineRunning = false;
        }

        if (isPoison)
        {
            if (!isPoisonRoutineRunning)
            {
                poisonEffect.GetComponent<ObjectAnimation>().PlayAnimation("Poison");
                poisonTimer = Random.Range(Mathf.RoundToInt(3 - (3 * PlayerManager.instance.negativeEffectReducer)), Mathf.RoundToInt(8 - (8 * PlayerManager.instance.negativeEffectReducer))); // Adjusted timer range for example
                StartCoroutine(OnPoisonRoutine());
                isPoisonRoutineRunning = true; // Ajout : marquer que la routine est en cours
            }
        }
        else
        {
            // Ajout : Réinitialisation si l'effet n'est plus actif
            isPoisonRoutineRunning = false;
        }
    }



    public void OnFire()
    {
        if (GetComponent<DestroyableBehiavor>() != null && Random.Range(0, 10) >= 6)
        {
            GetComponent<DestroyableBehiavor>().DestroyObject(9);
        }
        else if (GetComponent<LifeManager>() != null)
        {
            if(GetComponent<Stats>().entityType == EntityType.Monster)
                GetComponent<LifeManager>().TakeDamage(fireForce, Color.red);
            else if (GetComponent<Stats>().entityType == EntityType.Player)
                GetComponent<LifeManager>().TakeDamage(Mathf.RoundToInt(fireForce - (fireForce * PlayerManager.instance.negativeEffectReducer)), Color.red, true, true);

        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var hitCollider in hitColliders)
        {
            EntityEffects entityEffects = hitCollider.GetComponent<EntityEffects>();
            if (entityEffects != null)
            {
                if (Random.Range(0, 10) >= 6 && entityEffects.canBeFire && !entityEffects.isFire)
                {
                    entityEffects.SetState(isFire: true, force: fireForce);
                }
            }
        }
    }

    public void OnFreeze()
    {
        if (GetComponent<Stats>())
            GetComponent<Stats>().canMove = false;
    }

    public void ResetEffect()
    {
        StopCoroutine(OnFireRoutine());
        isFireRoutineRunning = false;
        fireTimer = 0;
        isFire = false;
        fireEffect.SetActive(false);
        fireEffect.GetComponent<ObjectParticles>().StopSpawningParticles();

        StopCoroutine(OnFreezeRoutine());
        isFreezeRoutineRunning = false;
        freezeTimer = 0;
        isFreeze = false;
        freezeEffect.SetActive(false);
        freezeEffect.GetComponent<ObjectParticles>().StopSpawningParticles();

        StopCoroutine(OnPoisonRoutine());
        isPoisonRoutineRunning = false;
        poisonTimer = 0;
        isPoison = false;
        poisonEffect.SetActive(false);
        poisonEffect.GetComponent<ObjectParticles>().StopSpawningParticles();

        StopCoroutine(OnSlimeRoutine());
        isSlimedRoutineRunning = false;
        slimeTimer = 0;
        isSlimed = false;
        slimeEffect.SetActive(false);
        slimeEffect.GetComponent<ObjectParticles>().StopSpawningParticles();
    }



    IEnumerator OnFireRoutine()
    {
        if (!GetComponent<Stats>() || (GetComponent<Stats>() && !GetComponent<Stats>().isDying))
        {

            isFireRoutineRunning = true;

            while (isFire)
            {
                yield return new WaitForSeconds(1);
                fireTimer--;

                OnFire();

                if (Random.Range(0, 10) >= 5)
                    fireEffect.GetComponent<ObjectParticles>().SpawnParticle("Smoke", transform.position, .2f, 5);

                if (Random.Range(0, 10) >= 2)
                    fireEffect.GetComponent<ObjectParticles>().SpawnParticle("Flames", transform.position, .3f, 10);


                if (fireTimer <= 0)
                {
                    fireEffect.GetComponent<ObjectAnimation>().StopAnimation();
                    isFire = false;
                    fireEffect.SetActive(false);
                }
            }

            isFireRoutineRunning = false;
        }
    }

    IEnumerator OnSlimeRoutine()
    {
        if (!GetComponent<Stats>() || (GetComponent<Stats>() && !GetComponent<Stats>().isDying))
        {

            isSlimedRoutineRunning = true;

            while (isSlimed)
            {
                yield return new WaitForSeconds(1);
                slimeTimer--;

                if (slimeTimer <= 0)
                {
                    isSlimed = false;
                    slimeEffect.SetActive(false);
                }

            }

            isSlimedRoutineRunning = false;
        }
    }

    IEnumerator OnFreezeRoutine()
    {
        if (!GetComponent<Stats>() || (GetComponent<Stats>() && !GetComponent<Stats>().isDying))
        {

            isFreezeRoutineRunning = true;

            while (isFreeze)
            {
                if (GetComponent<Stats>())
                    GetComponent<Stats>().canMove = false;

                freezeEffect.GetComponent<ObjectParticles>().SpawnParticle("Snowflakes", transform.position, .2f, 10);

                yield return new WaitForSeconds(1);
                freezeTimer--;

                OnFreeze();

                if (Random.Range(0, 10) >= 3)
                    freezeEffect.GetComponent<ObjectParticles>().SpawnParticle("Snowflakes", transform.position, .2f, 10);

                if (freezeTimer <= 0)
                {
                    isFreeze = false;
                    freezeEffect.SetActive(false);
                }

            }

            if (GetComponent<Stats>())
                GetComponent<Stats>().canMove = true;

            isFreezeRoutineRunning = false;
        }
    }

    IEnumerator OnPoisonRoutine()
    {
        if (!GetComponent<Stats>() || (GetComponent<Stats>() && !GetComponent<Stats>().isDying))
        {

            isPoisonRoutineRunning = true;

            while (isPoison)
            {
                yield return new WaitForSeconds(1);
                poisonTimer--;

                OnPoison();

                if (poisonTimer <= 0)
                {
                    isPoison = false;
                    poisonEffect.SetActive(false);
                }
            }

            isPoisonRoutineRunning = false;
        }
    }

    

    public void OnPoison()
    {
        GetComponent<LifeManager>().TakeDamage(GetComponent<Stats>().health / 40 + 1, Color.magenta, true, true);
    }
}
