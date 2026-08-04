using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioActiveIcePuzzle : MonoBehaviour
{
    [Header("Identifiant unique pour la sauvegarde (partagé par tous les cristaux du même puzzle)")]
    public string id;

    [Header("Désintégration")]
    [Tooltip("Nombre de valeurs possibles = startValue + 1 (ex: 3 => 3,2,1,0).")]
    public int startValue = 3;
    public float decayInterval = 2f;

    [Header("Apparence")]
    [Tooltip("Une couleur exacte par valeur possible : l'index dans la liste correspond directement à la valeur (index 0 = valeur 0/désintégré). Doit contenir au moins startValue + 1 entrées.")]
    public List<Color> valueColors = new List<Color> { Color.red, Color.yellow, Color.cyan, Color.white };

    [Header("Objets communs activés quand le puzzle est résolu (à renseigner identiquement sur chaque cristal du groupe)")]
    public List<GameObject> logicalEntites;

    [Header("Effet d'éclat sur changement de valeur")]
    public float flashHoldDuration = 0.25f;
    public float flashFadeDuration = 0.25f;

    private int currentValue;
    private bool isSolved;
    private SpriteRenderer sprite;
    private Coroutine flashCoroutine;

    private static readonly Dictionary<string, List<RadioActiveIcePuzzle>> groups = new Dictionary<string, List<RadioActiveIcePuzzle>>();

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(id)) return;

        if (!groups.TryGetValue(id, out var list))
        {
            list = new List<RadioActiveIcePuzzle>();
            groups[id] = list;
        }
        list.Add(this);
    }

    private void OnDisable()
    {
        if (string.IsNullOrEmpty(id)) return;

        if (groups.TryGetValue(id, out var list))
        {
            list.Remove(this);
            if (list.Count == 0) groups.Remove(id);
        }
    }

    private void Start()
    {
        sprite = GetComponentInChildren<SpriteRenderer>();

        bool savedState;
        if (SaveManager.instance.twoStateContainer.TryGetState(id, out savedState) && savedState)
        {
            Debug.Log($"[RadioActiveIcePuzzle:{id}] Puzzle déjà résolu (chargé depuis sauvegarde)");
            isSolved = true;
            currentValue = 0;
            UpdateVisual(false);
            ActivateLogicalEntities();
            enabled = false;
            return;
        }

        currentValue = Random.Range(0, startValue + 1);
        UpdateVisual(false);
        StartCoroutine(DecayRoutine());
    }

    // Chaque cristal a sa propre coroutine, mais Start() de tous les objets d'une scène
    // s'exécute pendant la même frame et ils partagent la même decayInterval : ils
    // décrémentent donc bien en même temps, sans "ticker" central arbitraire. La
    // vérification de synchro est repoussée d'une frame (yield return null) après le
    // décrément pour être sûr que TOUS les cristaux du groupe ont déjà appliqué leur
    // propre décrément de ce tic avant qu'on ne compare leurs valeurs entre eux.
    private IEnumerator DecayRoutine()
    {
        while (!isSolved)
        {
            yield return new WaitForSeconds(decayInterval);

            if (isSolved) yield break;

            if (currentValue > 0)
                currentValue--;
            else
                currentValue = startValue; // cycle déterministe : seul le décalage de départ est aléatoire

            UpdateVisual(false);
            GetComponent<SoundContainer>().PlaySound("Decay", 1);

            yield return null;

            if (!isSolved && AllGroupMembersAtZero())
            {
                SolvePuzzle();
            }
        }
    }

    // Décrément direct, indépendant du tic automatique (ne le retarde ni ne l'avance) :
    // c'est la prochaine vérification (n'importe quel cristal du groupe) qui verra la synchro.
    public void HitIce(int power)
    {
        if (isSolved || power <= 0)
            return;

        GetComponent<SoundContainer>().PlaySound("Hit", 1);

        currentValue = currentValue > 0 ? Mathf.Max(0, currentValue - power) : startValue;
        UpdateVisual();
    }

    private bool AllGroupMembersAtZero()
    {
        if (!groups.TryGetValue(id, out var list) || list.Count == 0)
            return false;

        foreach (var member in list)
        {
            if (member == null || member.currentValue != 0)
                return false;
        }
        return true;
    }

    private void SolvePuzzle()
    {
        SaveManager.instance.twoStateContainer.AddOrUpdateTemporaryState(id, true);
        Debug.Log($"[RadioActiveIcePuzzle:{id}] Puzzle résolu !");

        if (groups.TryGetValue(id, out var list))
        {
            // Copie défensive : geler un membre déclenche OnDisable, qui modifierait
            // la liste d'origine pendant qu'on la parcourt.
            var members = new List<RadioActiveIcePuzzle>(list);
            foreach (var member in members)
            {
                if (member == null) continue;

                member.isSolved = true;
                member.currentValue = 0;
                member.UpdateVisual(false);
                member.StopAllCoroutines();
                member.enabled = false;
            }
        }
        else
        {
            isSolved = true;
        }

        // Ne se joue qu'une fois : le premier cristal à détecter la synchro appelle
        // SolvePuzzle() et gèle tous les autres avant qu'ils ne puissent le refaire.
        GetComponent<SoundContainer>().PlaySound("Solved", 1);

        UpdateEventContainer();
        if (logicalEntites.Count > 0 && TryGetComponent<EventPlayer>(out var eventPlayer))
        {
            eventPlayer.PlayAnimation();
        }

        ActivateLogicalEntities();
    }

    private void UpdateVisual(bool flash = true)
    {
        if (sprite == null || valueColors == null || valueColors.Count == 0) return;

        int index = Mathf.Clamp(currentValue, 0, valueColors.Count - 1);
        Color target = valueColors[index];
        target.a = sprite.color.a;

        if (!flash)
        {
            sprite.color = target;
            return;
        }

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashThenFade(target));
    }

    private IEnumerator FlashThenFade(Color target)
    {
        Color white = Color.white;
        white.a = target.a;
        sprite.color = white;

        yield return new WaitForSeconds(flashHoldDuration);

        float elapsed = 0f;
        while (elapsed < flashFadeDuration)
        {
            elapsed += Time.deltaTime;
            sprite.color = Color.Lerp(white, target, elapsed / flashFadeDuration);
            yield return null;
        }

        sprite.color = target;
        flashCoroutine = null;
    }

    private void UpdateEventContainer()
    {
        if (logicalEntites == null || logicalEntites.Count == 0 || logicalEntites[0] == null)
            return;

        Vector3 logicPos = logicalEntites[0].transform.position;
        Vector3 selfPos = transform.position;
        Vector3 relative = logicPos - selfPos;

        if (TryGetComponent<EventPlayer>(out var eventPlayer))
        {
            eventPlayer.eventContainer = EventGeneratorManager.instance.MoveCamera(Vector2.zero, relative, 1f, 2f);
        }
    }

    private void ActivateLogicalEntities()
    {
        foreach (GameObject entity in logicalEntites)
        {
            if (entity == null) continue;

            if (entity.GetComponent<DoorBehiavor>() is DoorBehiavor door)
            {
                if (!door.isOpen) door.OpenDoor();
            }
            else if (entity.GetComponent<Spades>() is Spades spades)
            {
                StartCoroutine(spades.RoutineSpades());
            }
            else if (entity.GetComponent<SkeletonBridgeBehiavor>() is SkeletonBridgeBehiavor bridge)
            {
                bridge.Activate();
            }
            else if (entity.GetComponent<RustCircleBehiavor>() is RustCircleBehiavor rustCircle)
            {
                rustCircle.TurnOn();
            }
            else
            {
                entity.SetActive(true);
            }
        }
    }
}
