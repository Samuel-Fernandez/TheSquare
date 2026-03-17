# Documentation du Système de Sceaux (Seal System)

Le système de Sceaux (Seals) est une mécanique d'alchimie où le joueur combine des objets spécifiques pour créer un Sceau. Ce Sceau confère diverses améliorations (stats, effets passifs ou actifs) basées sur la composition élémentaire (Essences/Spirits) des objets utilisés.

## 1. Fonctionnement Global

La création d'un Sceau nécessite **exactement 4 Objets Spéciaux** (`SpecialItems`). 
Chaque objet possède une **Composition d'Essence** (`EssenceComposition`), c'est-à-dire un certain pourcentage de divers Esprits (`Spirit` - ex: Feu, Eau, Neutre...).

Lors de la fusion (`Seal.GenerateSeal`) :
1. Les compositions des 4 objets sont moyennées (chaque objet compte pour un quart du sceau final).
2. Le sceau hérite d'une composition globale en pourcentages d'essences (ex: 50% Feu, 25% Terre, 25% Vent).
3. Ces pourcentages sont utilisés pour **multiplier** les statistiques de base de chaque `Spirit` afin de déterminer les statistiques finales du sceau.
4. Des **Archétypes** sont activés si la somme des "votes" des essences du sceau atteint un certain seuil.

## 2. Les Essences (Spirits)

Il existe actuellement 8 "Spirits" ou éléments distincts gérés par l'interface :
- **Neutral** (Neutre)
- **Fire** (Feu)
- **Ground** (Terre)
- **Light** (Lumière)
- **Shadow** (Ombre)
- **Vegetal** (Végétal)
- **Water** (Eau)
- **Wind** (Vent)

Chaque objet `Spirit` (ScriptableObject) définit ce qu'il apporte **pour chaque pourcent** de sa présence dans le sceau. Par exemple, si l'Esprit du Feu donne `forcePerPercent = 1`, et que le sceau est composé à 50% de Feu, le sceau octroiera `0.5` de Force supplémentaire.

De plus, chaque Esprit détient des **"Votes"** pour les 4 Archétypes (ex: `votesResonance`, `votesBuff`, etc.), ce qui détermine l'orientation du sceau.

## 3. Les 4 Archétypes

Un sceau est divisé en 4 archétypes de capacités. Un archétype est **actif** uniquement si son **Score** (la somme des pourcentages d'essences multipliée par leurs votes respectifs) est **supérieur ou égal à 1.5**. 
*(Remarque : Un log dans le code indique que le seuil original ou visé était peut-être 0.75, mais le code actuel vérifie `>= 1.5f`)*.

Un même sceau peut posséder plusieurs archétypes actifs simultanément.

### A. BUFF (Améliorations Passives)
L'archétype Buff confère des augmentations de statistiques passives au joueur :
- Points de vie (HP)
- Force (Str)
- Défense (Def)
- Vitesse (Spe)
- Dégâts et Chances de Coup Critique (Crit Dmg / Crit Chance)
- Vitesse d'attaque à l'Arc et à la Pioche
- Chances d'esquive (Dodge Chance)

### B. RESONANCE (Effets Déclenchés)
L'archétype Résonance ajoute un effet spécial qui a une chance de se déclencher :
- **Effets possibles** (`ResonanceEffectType`) : Explosion, StopEntity, Electricity. *L'effet choisi est celui de l'esprit dominant ayant voté pour l'archétype Résonance.*
- **Statistiques gérées** :
  - Rayon de la résonance (`resonanceRadius`)
  - Chance d'activation (`activationChancePercent`)
  - Puissance/Magnitude de l'effet (`effectMagnitudePercent`)

### C. AURA (Zone d'Effet)
L'archétype Aura génère une zone d'effet autour du joueur agissant sur la durée (Aura Radius, Duration, Tick Rate) :
- **Effets sur le joueur** : Régénération HP, augmentation de Défense et de Force par "tick".
- **Effets sur les ennemis** : Ralentissement (Enemy Slow), Dégâts par tick.

### D. MOMENTUM (Système d'Accumulation / Stacks)
L'archétype Momentum est un système de "Stacks" (cumuls) qui se déclenchent sous certaines conditions et s'estompent avec le temps.
- **Déclencheurs** (`MomentumTriggerType`) : OnKill (À la mort d'un ennemi), OnDamageTaken (Dégâts reçus), OnCrit (Coup critique), OnPerfectDodge (Esquive parfaite), OnMineralMined (Minerai miné), OnBowUsing (Utilisation de l'arc). *Le déclencheur est défini par l'esprit dominant votant pour Momentum.*
- **Mécanique** :
  - Stacks maximums (`maxStacks`)
  - Vitesse de dissipation des Stacks (`stackDecay`)
- **Bonus par Stack** : Force, Défense, Vitesse, Pièces carrées (Square Coins), Vitesse d'Arc/Pioche, Chances d'esquive.

## 4. Architecture Globale et Intégration

Afin de ne pas surcharger les classes vitales (comme `PlayerController`), le système repose sur des composants dédiés à chaque archétype :

1. **Archétype BUFF :** Intégré directement dans `Stats.cs`. Les valeurs (pourcentages) s'appliquent après le calcul des statistiques de base et de l'équipement. TERMINE
2. **Archétype AURA :** Un script dédié (`SealAuraManager`) attaché au joueur, qui s'activera pour lancer un calcul de zone (`OverlapCircle/OverlapSphere`) tous les `auraTickRate` secondes. TERMINE
3. **Archétype MOMENTUM :** Un script dédié (`SealMomentumManager`) qui écoutera les événements en jeu. Il gèrera le système de charges (Stacks) et leur temps d'expiration (`stackDecay`). TERMINE
4. **Archétype RESONANCE :** Un script dédié (`SealResonanceManager`) attaché au joueur, qui gère la probabilité de déclenchement via `TryTriggerResonance(Vector3)`. Il instancie des prefabs d'effets (Explosion, Glace, Électricité). Ces prefabs utilisent le script de base `ResonanceEffectBehavior` qui reçoit les données du sceau (rayon, magnitude) pour s'adapter visuellement (taille, couleur) et appliquer les effets. TERMINE

## 5. L'Interface (UI)

- **`SealUI` / Boutons** : L'interface permet de sélectionner les `SpecialItems`. Les 4 gros boutons affichent la couleur de l'essence dominante de l'objet inséré. 
- **`SealDescriptionUI`** : Ce script gère l'affichage dynamique des propriétés du sceau. Il masque automatiquement les archétypes inactifs et tous les textes liés à des statistiques à zéro. Il liste également la répartition des éléments (seulement s'ils sont > 0%).

## 6. État Actuel et À Venir (WIP)

### Noms des Sceaux (Génération Procédurale)
Dans `SealManager.cs`, la fonction `GenerateSealName()` renvoie actuellement en dur `"no name"`.  
**Objectif futur** : Implémenter une génération de noms procédurale. L'idéal sera d'attribuer des mots-clés pré-définis à certains `Spirits` ou certains Archétypes dominants (ex: l'archétype Feu + Résonance donne le préfixe "Explosif", etc.) pour générer des noms du type "Sceau Explosif du Vent".

### Sauvegarde
La liste `createdSeals` dans `SealManager` permet (ou permettra) de stocker tous les sceaux fabriqués par le joueur afin de les conserver en mémoire.

---
*Ce document sert de référence technique pour l'état actuel de l'implémentation des Sceaux et pourra être étendu au fur et à mesure du développement, notamment avec la génération de noms procéduraux.*
