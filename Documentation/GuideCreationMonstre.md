# Guide de Création de Nouveaux Monstres (Modèles de Comportement)

Ce document synthétise les pratiques et structures communes observées dans les scripts de comportements des monstres existants (`*Behiavor.cs`), tels que `SporkBehiavor`, `PoisonPlantBehiavor`, `SlimeCyclopeBehiavor` ou encore `SnakeBehiavor`. Son but est de servir de modèle lors de la création de nouvelles entités monstrueuses.

---

## 1. Composants Indispensables (Dépendances)
Généralement, le script de comportement ne gère pas tout seul l'entité. Il coopère avec plusieurs autres composants souvent rattachés au même GameObject :
- **`Stats`** : Utilisé pour la gestion la vie, l'invulnérabilité (`isVulnerable`), et parfois la puissance de l'attaque (`strength`), ainsi que pour distinguer facilement qu'il s'agit d'un monstre (`entityType == EntityType.Monster`).
- **`NewMonsterMovement`** : C'est le composant privilégié de détection (`IsInDetectionZone`) et de déplacement. Il doit souvent voir ses animations suspendues lors d'une attaque (`EnableAnimations = false`).
- **`ObjectAnimation`** : Permet de déclencher les animations de manière propre (ex: `PlayAnimation("Idle")`, ou via coroutine `PlayAnimationCoroutine("Appear", true)`).
- **`SoundContainer`** : Très utilisé pour tous les effets sonores liés aux actions ("Appear", "Attack", "Spit", "Explosion").

## 2. Détection du Joueur
Deux méthodes principales coexistent pour repérer le joueur :
- **Via `NewMonsterMovement` (Le plus moderne/recommandé)** : 
  On lit simplement le booléen `monsterMovement.IsInDetectionZone`. Pratique si le monstre tourne autour ou traque le joueur.
- **Via Distance Manuelle** :
  Des monstres comme le *Spork* ou la *PoisonPlant* calculent la distance `Vector2.Distance` par rapport à `PlayerManager.instance.player.transform.position`. Ils vérifient également qu'ils sont bien apparus (`isAppeared`) avant d'agir.

## 3. Structures par Coroutines (IEnumerator)
Les actions du monstre qui s'étalent sur la durée (apparitions, attaques, cooldowns) sont presque toujours structurées sous forme de **Coroutines**. 
**Modèle classique** :
```csharp
IEnumerator RoutineDAttaque()
{
    isAttacking = true; // Empêche le déclenchement en boucle

    // 1 - Animation de pré-attaque + Son
    GetComponent<ObjectAnimation>().PlayAnimation("PrépareAttaque");
    GetComponent<SoundContainer>().PlaySound("Charge", 1);
    yield return new WaitForSeconds(0.5f); // Temps de cast

    // 2 - Déclenchement de l'attaque (Dégâts, spawn projectil, etc...)
    // ... code d'attaque ...

    // 3 - Retour à la normale
    GetComponent<ObjectAnimation>().PlayAnimation("Idle");
    yield return new WaitForSeconds(cooldownTime); // Cooldown

    isAttacking = false;
}
```

*Attention aux nettoyages* : Toujours prévoir l'arrêt pur et simple des coroutines (via `StopCoroutine`) en cas de disparition, de réinitialisation ou de mort du monstre, et de bien remettre les booléens de states (ex: `isAttacking`, `stats.doingAttack`) à `false`.

## 4. Offensives : Gestion des dégâts
La notion d'attaque peut prendre plusieurs formes qui dépendent du type de monstre :
- **Tirs (Spit/Projectile)** : Le monstre instancie un GameObject `ProjectilePrefab` au niveau d'un point d'ancrage (`spitSpawn.transform.position`), et l'initialise avec l'angle ciblant le joueur via `projectileInstance.GetComponent<ProjectileBehavior>().InitProjectile(...)`.
- **Zones Actives (AoE/Hitbox)** : Utile pour des monstres statiques (ex: *PoisonPlant* avec sa `damageZone`). On utilise `SetActive(true)`/`SetActive(false)` sur un objet enfant muni de `DamageZoneBehiavor`.
- **Dégâts Directs** : Exécutés via `PlayerManager.instance.player.GetComponent<LifeManager>().TakeDamage(...)` à la fin d'une animation si les conditions (ex: contact ou Line of Sight) sont réunies.
- **Contact Collider (`OnCollisionStay2D`)** : `LifeManager` inflige automatiquement des dégâts au joueur quand le collider d'un monstre reste en contact avec le sien, **mais uniquement si `stats.doingAttack` vaut `true`** au moment du contact. Un monstre qui se contente de patrouiller ou de chasser sans attaquer ne blesse donc plus le joueur au simple toucher. Attention : `doingAttack` n'est pas utilisé que pour ça, il pilote aussi d'autres systèmes (ex: la détection d'esquive côté joueur dans `PlayerController`) — le garder cohérent avec l'état réel d'attaque du monstre profite à tout le monde, pas seulement aux dégâts de contact.
  - **Monstres à pattern d'attaque** (la majorité) : `doingAttack` doit être mis à `true` dès le début de la fenêtre d'attaque réelle (le moment où le contact doit faire mal) et remis à `false` dès la fin de cette fenêtre — comme tout autre booléen de state (`isAttacking`), il doit aussi être remis à `false` en cas d'interruption/mort (voir section 3).
  - **Monstres "contact-only"** (dangereux en permanence, sans fenêtre d'attaque dédiée — ex: hasards ambiants type Chuchu) : cocher `alwaysDoingAttack` sur le `Stats` du prefab plutôt que de gérer `doingAttack` manuellement. `Stats.Update()` force alors `doingAttack = true` en permanence pour cette entité, donc tous les systèmes qui lisent `doingAttack` (dégâts de contact, esquive, etc.) la traitent comme toujours en train d'attaquer — pas seulement `LifeManager`.
  - Un monstre qui ne positionne ni `doingAttack` ni `alwaysDoingAttack` ne fera plus aucun dégât de contact — à vérifier systématiquement lors de la création d'un nouveau monstre.
- **Altération d'États (Poison, etc.)** : Implémenté via les collisions `OnCollisionEnter2D`, en appelant `EntityEffects.SetState(...)` sur l'objet percuté s'il s'agit du joueur.

## 5. Interactions Visuelles (Feedbacks)
Le comportement ne s'arrête pas à la mécanique de dégâts :
- **Orientation (Flip)** : Si le sprite du monstre doit regarder vers le joueur, on compare sa position `X` avec le joueur et l'on modifie `spriteRenderer.flipX`.
- **Fade In/Out** : Les changements comme les disparitions progressives (ex: le *Snake* ou le fantôme) gèrent le fondu de visibilité à l'aide de coroutines (Lerp sur `spriteRenderer.color.a`).
- **Caméra** : Les secousses (`ShakeCamera`) ou les filtres modifiés (`SetFilter`) via le singleton `CameraManager.instance` participent aux "gros monstres" pour impacter fortement la perception d'une attaque violente.

## 6. Bonnes Pratiques Complémentaires
- Toujours vérifier que le joueur existe dans les `Update` (`if (PlayerManager.instance?.player == null) return;`).
- Inclure de l'aide au Game Design avec la fonction Unity native `OnDrawGizmosSelected` pour dessiner dans l'Editeur les cercles de portée (`Gizmos.DrawWireSphere()`).
- Si une animation interrompt un déplacement automatique géré par `NewMonsterMovement`, assurez-vous de désactiver puis réactiver les animations via `EnableAnimations = false` puis `EnableAnimations = true`.
