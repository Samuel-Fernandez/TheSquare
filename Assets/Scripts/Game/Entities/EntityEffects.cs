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


    // ----------- AURA ----------- 
    public bool isAuraSlowed;
    public float auraSlowPercentage;
    public float auraSlowTimer;
    // ----------------------------
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

    // Stockage des r�f�rences aux coroutines pour pouvoir les arr�ter proprement
    private Coroutine fireCoroutine;
    private Coroutine freezeCoroutine;
    private Coroutine poisonCoroutine;
    private Coroutine slimeCoroutine;

    private void Update()
    {

        DetermineState();

        if (GetComponent<Stats>() && GetComponent<Stats>().isDying)
        {
            ResetEffect();
        }

        // Gestion du ralentissement de l'Aura
        if (isAuraSlowed)
        {
            auraSlowTimer -= Time.deltaTime;
            if (auraSlowTimer <= 0)
            {
                isAuraSlowed = false;
                auraSlowPercentage = 0f;
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isFireRoutineRunning = false;
        isFreezeRoutineRunning = false;
        isPoisonRoutineRunning = false;
        isSlimedRoutineRunning = false;
    }

    // Permet d'inverser la possibilit� d'�tre sensible � un effet
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

            if (canBeSlimed)
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

        if (isSlimed)
        {
            if (!isSlimedRoutineRunning)
            {
                slimeEffect.GetComponent<ObjectAnimation>().PlayAnimation("Slime");
                slimeTimer = Random.Range(Mathf.RoundToInt(5 - (5 * PlayerManager.instance.negativeEffectReducer)), Mathf.RoundToInt(8 - (8 * PlayerManager.instance.negativeEffectReducer)));
                slimeCoroutine = StartCoroutine(OnSlimeRoutine());
                isSlimedRoutineRunning = true;
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
                fireTimer = Random.Range(Mathf.RoundToInt(5 - (5 * PlayerManager.instance.negativeEffectReducer)), Mathf.RoundToInt(10 - (10 * PlayerManager.instance.negativeEffectReducer)));
                fireCoroutine = StartCoroutine(OnFireRoutine());
                isFireRoutineRunning = true;
            }
        }
        else
        {
            isFireRoutineRunning = false;
        }

        if (isFreeze)
        {
            if (!isFreezeRoutineRunning)
            {
                freezeEffect.GetComponent<ObjectAnimation>().PlayAnimation("Freeze");
                freezeEffect.GetComponent<SoundContainer>().PlaySound("Freeze", 2);
                freezeTimer = Random.Range(Mathf.RoundToInt(3 - (3 * PlayerManager.instance.negativeEffectReducer)), Mathf.RoundToInt(6 - (6 * PlayerManager.instance.negativeEffectReducer)));
                freezeCoroutine = StartCoroutine(OnFreezeRoutine());
                isFreezeRoutineRunning = true;
            }
        }
        else
        {
            isFreezeRoutineRunning = false;
        }

        if (isPoison)
        {
            if (!isPoisonRoutineRunning)
            {
                poisonEffect.GetComponent<ObjectAnimation>().PlayAnimation("Poison");
                poisonTimer = Random.Range(Mathf.RoundToInt(3 - (3 * PlayerManager.instance.negativeEffectReducer)), Mathf.RoundToInt(8 - (8 * PlayerManager.instance.negativeEffectReducer)));
                poisonCoroutine = StartCoroutine(OnPoisonRoutine());
                isPoisonRoutineRunning = true;
            }
        }
        else
        {
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
            if (GetComponent<Stats>().entityType == EntityType.Monster)
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
        // Arr�ter Fire
        if (fireCoroutine != null)
            StopCoroutine(fireCoroutine);
        isFireRoutineRunning = false;
        fireTimer = 0;
        isFire = false;
        if (fireEffect)
        {
            fireEffect.SetActive(false);
            fireEffect.GetComponent<ObjectParticles>().StopSpawningParticles();
        }

        // Arr�ter Freeze
        if (freezeCoroutine != null)
            StopCoroutine(freezeCoroutine);
        isFreezeRoutineRunning = false;
        freezeTimer = 0;
        isFreeze = false;
        if (freezeEffect)
        {
            freezeEffect.SetActive(false);
            freezeEffect.GetComponent<ObjectParticles>().StopSpawningParticles();
        }

        // Arr�ter Poison
        if (poisonCoroutine != null)
            StopCoroutine(poisonCoroutine);
        isPoisonRoutineRunning = false;
        poisonTimer = 0;
        isPoison = false;
        if (poisonEffect)
        {
            poisonEffect.SetActive(false);
            poisonEffect.GetComponent<ObjectParticles>().StopSpawningParticles();
        }

        // Arr�ter Slime
        if (slimeCoroutine != null)
            StopCoroutine(slimeCoroutine);
        isSlimedRoutineRunning = false;
        slimeTimer = 0;
        isSlimed = false;
        if (slimeEffect)
        {
            slimeEffect.SetActive(false);
            slimeEffect.GetComponent<ObjectParticles>().StopSpawningParticles();
        }

        // R�activer le mouvement si gel�
        if (GetComponent<Stats>())
            GetComponent<Stats>().canMove = true;
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

    public void StopFire()
    {
        if (fireCoroutine != null)
            StopCoroutine(fireCoroutine);
        isFireRoutineRunning = false;
        fireTimer = 0;
        isFire = false;
        if (fireEffect)
        {
            fireEffect.SetActive(false);
            fireEffect.GetComponent<ObjectParticles>().StopSpawningParticles();
            fireEffect.GetComponent<ObjectAnimation>().StopAnimation();
        }
    }

    public void StopSlime()
    {
        if (slimeCoroutine != null)
            StopCoroutine(slimeCoroutine);
        isSlimedRoutineRunning = false;
        slimeTimer = 0;
        isSlimed = false;
        if (slimeEffect)
        {
            slimeEffect.SetActive(false);
            slimeEffect.GetComponent<ObjectParticles>().StopSpawningParticles();
            slimeEffect.GetComponent<ObjectAnimation>().StopAnimation();
        }
    }

    public void StopFreeze()
    {
        if (freezeCoroutine != null)
            StopCoroutine(freezeCoroutine);
        isFreezeRoutineRunning = false;
        freezeTimer = 0;
        isFreeze = false;
        if (freezeEffect)
        {
            freezeEffect.SetActive(false);
            freezeEffect.GetComponent<ObjectParticles>().StopSpawningParticles();
            freezeEffect.GetComponent<ObjectAnimation>().StopAnimation();
        }
        if (GetComponent<Stats>())
            GetComponent<Stats>().canMove = true;
    }

    public void StopPoison()
    {
        if (poisonCoroutine != null)
            StopCoroutine(poisonCoroutine);
        isPoisonRoutineRunning = false;
        poisonTimer = 0;
        isPoison = false;
        if (poisonEffect)
        {
            poisonEffect.SetActive(false);
            poisonEffect.GetComponent<ObjectParticles>().StopSpawningParticles();
            poisonEffect.GetComponent<ObjectAnimation>().StopAnimation();
        }
    }

    public void ApplyAuraSlow(float percentage, float duration)
    {
        isAuraSlowed = true;
        // On garde le pire ralentissement si déjà ralenti
        if (percentage > auraSlowPercentage)
            auraSlowPercentage = percentage;

        // On réinitialise ou prolonge le timer
        auraSlowTimer = Mathf.Max(auraSlowTimer, duration);
    }
}