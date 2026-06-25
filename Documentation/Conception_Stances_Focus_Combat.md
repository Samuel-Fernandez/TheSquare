# Conception : Système de Stances et Runes de Combat

TODO :
- FAIRE SPRITES DES STANCES ET DES RUNES (NEUTRE DEJA FAIT)

Ce document décrit le design et le fonctionnement de la mécanique de **Stances et Runes de Combat** (anciennement Skills/Focus).

## 1. Description du Concept
La mécanique permet au joueur d'adapter dynamiquement son type d'attaque (la **Posture**) et les conditions tactiques d'application de ses dégâts (les **Runes**).
Le joueur utilise deux cases distinctes situées dans les coins supérieurs de l'écran (HUD) :
* **En haut à gauche** : La **Posture de Combat** (changement circulaire via Gâchette Gauche / `LT` ou une touche clavier comme `Q`/`A`).
* **En haut à droite** : La **Rune de Combat** (changement circulaire via Gâchette Droite / `RT` ou une touche clavier comme `E`/`D`).

---

## 2. Les Postures de Combat (Top-Left HUD)
Les postures déterminent le type physique de l'attaque. Elles adaptent les dégâts selon la **constitution** du monstre ciblé.

| Posture | Type d'ennemi ciblé | Exemples de Monstres | Multiplicateur suggéré |
| :--- | :--- | :--- | :--- |
| **Posture Neutre / Adaptative** | Boss complexes ou entités uniques | Araxie, Droxen | **x1.0** (Dégâts constants) |
| **Posture Tranchante** | Végétaux, organiques charnus | Poison Plant, Prêtre Corrompu | **x1.5** |
| **Posture Contondante** | Squelettiques, Minéraux et Cuirassés | Skeletons, Skeleton King, Gargouille | **x1.5** |
| **Posture Perforante** | Amorphes, fluides, gélatineux | Blob, Slime Aberration | **x1.5** |
| **Posture Spirituelle** | Magiques, immatériels | Wizard Skeleton, Spectres | **x1.5** |
| **Posture Anti-Square** | Entités de The Square | Amalgames maudits, Racines corrompues | **x1.5** |

---

## 3. Les Runes de Combat (Top-Right HUD)
Les runes déterminent le style de combat actif. Elles appliquent un multiplicateur ou un effet en fonction des **actions ou de l'état du joueur**, et s'appuient strictement sur les statistiques du personnage (`Stats.cs`).

### 1. Rune de Combat (Standard)
* **Description** : L'état d'esprit de base du guerrier, sans prise de tête.
* **Effet** : Bonus de force (`strength`) passif et constant.

### 2. Rune de Rage (Témérité)
* **Description** : Plus les points de vie du joueur sont bas, plus ses attaques sont dévastatrices.
* **Effet** : Augmente la force (`strength`) proportionnellement aux PV (`health`) manquants du joueur.

### 3. Rune d'Élan (Fluidité)
* **Description** : Canalise l'énergie cinétique du joueur.
* **Effet** : Bonus temporaire de force (`strength`) après avoir effectué une esquive (*Dodge*) ou en attaquant en plein mouvement.

### 4. Rune de Sacrifice
* **Description** : Frappe avec l'énergie de sa propre vitalité.
* **Effet** : Consomme un pourcentage fixe de PV (`health`) à chaque attaque pour infliger un coup à la force (`strength`) massivement augmentée.

### 5. Rune de Repoussoir (Impact)
* **Description** : Privilégie la distance de sécurité en écartant les adversaires.
* **Effet** : Augmente drastiquement la statistique `knockbackPower` pour repousser violemment les ennemis, mais réduit la `speed`.

### 6. Rune de Forteresse (Colosse)
* **Description** : Le joueur devient un véritable tank inamovible.
* **Effet** : Augmente significativement la `defense` et la `knockbackResistance`, mais réduit fortement la vitesse de déplacement (`speed`).

### 7. Rune de Vélocité (Vif-Argent)
* **Description** : Concentre tout sur la mobilité au détriment de la protection.
* **Effet** : Booste la `speed` de déplacement du joueur, au prix d'une diminution de sa `defense`.

### 8. Rune de Fortune (Avarice)
* **Description** : Attire la chance et les opportunités.
* **Effet** : Augmente massivement la statistique `luck` (influe sur les drops ou événements de chance) mais réduit la force de base (`strength`).

### 9. Rune d'Archerie (Tireur)
* **Description** : Spécialisation dédiée au combat à distance.
* **Effet** : Confère un grand bonus de dégâts (`strength`) uniquement lors de l'utilisation de l'arc (état `isBowShooting` actif).

### 10. Rune de Létalité (Critique)
* **Description** : Cherche systématiquement les points vitaux.
* **Effet** : Augmente les chances de critique (`critChance`) et les dégâts critiques (`critDamage`), au détriment de la `knockbackResistance` (le joueur est plus facilement interrompu).

---

## 4. Multiplicateur Combiné (Synergie)
L'intérêt majeur de cette mécanique est l'effet multiplicatif entre la **Posture** et la **Rune**.

### Formule de Calcul
$$\text{Dégâts Finaux} = \text{Dégâts de Base} \times \text{Modificateur Posture} \times \text{Modificateur Rune}$$

### Exemple de combat
1. Le joueur affronte un **Skeleton** (type Squelettique) -> Il passe en **Posture Contondante** (Dégâts x1.5).
2. Le joueur effectue un **Dodge** parfait et contre-attaque immédiatement avec la **Rune d'Élan** active (Dégâts x1.4).
3. Le multiplicateur cumulé est : $1.5 \times 1.4 = 2.1$ (soit **+110% de dégâts**).

---

## 5. Intégration Visuelle et Contrôles
* **HUD** : Deux cases épurées affichant une icône stylisée et une couleur thématique. À chaque rotation, la case correspondante s'anime brièvement (léger effet de zoom et impulsion lumineuse).
* **Effet en jeu** : Pas d'effet visuel encombrant sur l'épée, mais une lueur colorée très subtile (2D Light) émane du joueur lors de la frappe en fonction de la posture ou rune active.
