# Conception : Système de Stances et Runes de Combat

TODO :
- FAIRE SPRITES DES STANCES ET DES RUNES (NEUTRE DEJA FAIT)

Ce document décrit le design et le fonctionnement de la mécanique de **Stances et Runes de Combat** (anciennement Skills/Focus).

## 1. Description du Concept
La mécanique permet au joueur d'adapter dynamiquement son type d'attaque (la **Posture**) et les conditions tactiques d'application de ses dégâts (les **Runes**).
Le joueur utilise deux éléments distincts situés dans les coins supérieurs de l'écran (HUD) :
* **En haut à gauche** : La **Posture de Combat** (changement circulaire via Gâchette Gauche / `LT` ou une touche clavier comme `Q`/`A`).
* **En haut à droite** : La **Rune de Combat Active** (changement circulaire parmi un **Deck de 3 Runes pré-équipées** via Gâchette Droite / `RT` ou une touche clavier comme `E`/`D`).

---

## 2. Les Postures de Combat (Top-Left HUD)
Les postures déterminent le type physique de l'attaque. Elles adaptent les dégâts selon la **constitution** du monstre ciblé.

Posture,Cible (+50%),Malus (-50%),Description en jeu
Neutre,Toutes,Aucun,"Dégâts standards (x1.0). La posture par défaut : aucun point faible, mais aucune fulgurance."
Tranchante,Amorphes,Rigides,+50% de dégâts pour cisailler les masses instables. -50% face aux blindages et à la roche où la lame rebondit misérablement.
Contondante,Rigides,Amorphes,+50% de dégâts pour fracasser les os et le métal. -50% contre la gélatine qui se contente d'absorber l'impact.
Perforante,Charnus,Éthérés,+50% de dégâts d'estoc dans les organes vitaux et la sève. -50% face aux spectres : essayer de piquer du vent est stupide.
Spirituelle,Éthérés,Charnus,"+50% de dégâts perturbant les entités astrales. -50% sur la chair brute et les plantes, dont la force vitale rejette ce type de résonance."
Anti-Square,Anomalies,Toutes,"+50% d'annihilation ciblée sur les entités de The Square. -50% contre tout le reste. Dévastateur sur sa cible, suicidaire à l'aveugle."

---

## 3. Les Runes de Combat (Top-Right HUD)
Les runes déterminent le style de combat actif. Elles appliquent un multiplicateur ou un effet en fonction des **actions ou de l'état du joueur**, et s'appuient strictement sur les statistiques du personnage (`Stats.cs`).

### 1. Rune de Combat (Standard) OK
* **Description** : L'état d'esprit de base du guerrier, sans prise de tête.
* **Effet** : Bonus de force +10% passif et constant.

### 2. Rune de Rage (Témérité) OK
* **Description** : Plus les points de vie du joueur sont bas, plus ses attaques sont dévastatrices.
* **Effet** : +1% de dégats pour chaque 1% de vie perdu.

### 3. Rune d'Élan (Fluidité) OK
* **Description** : Canalise l'énergie cinétique du joueur.
* **Effet** : +20% de dégats pendant 1 seconde après avoir effectué une esquive parfaite

### 4. Rune de Sacrifice
* **Description** : Frappe avec l'énergie de sa propre vitalité.
* **Effet** : Consomme 10% de PV pour infliger 50% de dégats supplémentaires.

### 5. Rune de Plénitude (Sérénité)
* **Description** : Le joueur tire sa force d'une vitalité intacte.
* **Effet** : +25% de force, +25% de chance critique, +25% de dégâts critiques si la vie du joueur est à 100%.

### 6. Rune de Triomphe (Second Souffle)
* **Description** : Chaque ennemi vaincu nourrit l'ardeur du joueur.
* **Effet** : Restaure instantanément 5% des PV Max de l'entité tuée au joueur.

### 7. Rune de Prospérité (Aubaine)
* **Description** : Les frappes précises libèrent la richesse cachée des monstres.
* **Effet** : Chaque coup critique infligé génère le nombre de dégâts en square coins

### 8. Rune de Surtension (Opportunisme) OK
* **Description** : Profite des failles de l'adversaire pour frapper plus fort.
* **Effet** : +50% de dégâts contre les entités subissant un effet (Glace, feu, poison...)

### 9. Rune d'Instabilité (Chaos)
* **Description** : Laisse la fortune décider de la violence de vos frappes.
* **Effet** : Applique un modificateur de dégâts aléatoire compris entre `[-50% + luck]%` et `[+50% + luck]%` (inclus) à chaque attaque.

### 10. Rune de Mimétisme (Résonance)
* **Description** : Renforce la résonance de la posture adoptée pour percer les défenses.
* **Effet** : Double le multiplicateur actif de la Posture de combat active (ex: un multiplicateur de x1.5 devient x3.0). Le malus est aussi touché (-50% -> -75%)

### 11. Rune de Tempête (Météorologie)
* **Description** : Harmonise la rage du joueur avec le déchaînement du ciel.
* **Effet** : Sous la pluie, le blizzard, la tempête de sable etc... augmente les dégâts critiques (`critDamage`) de 50%. S'il fait beau, confère +15% de chances de coup critique (`critChance`).

### 12. Rune de Surcharge (Double Tranchant)
* **Description** : Expose volontairement vos points faibles pour porter un coup fatal.
* **Effet** : Le joueur subit +30% de dégâts supplémentaires, mais ses dégâts physiques globaux sont augmentés de +40% et ses chances de coup critique (`critChance`) augmentent de +20%.

### 13. Rune de Conversion (Brise-Blindage) OK
* **Description** : Convertit l'acier de votre armure en énergie offensive pure.
* **Effet** : La moitié de vos points de défense (`defense`) actuels est retirée/convertie. Pour chaque point de défense converti, vous gagnez +5% de force (`strength`).

### 14. Rune d'Éclipse (Ombre et Lumière)
* **Description** : Adapte le style de combat à la clarté du monde.
* **Effet** : Confère +20% de défense (`defense`) de nuit ou dans les caves et donjons, mais applique un malus de -20% de défense le jour.

### 15. Rune d'Encerclement (Survie de Groupe)
* **Description** : La présence d'adversaires multiples aiguise vos sens de guerrier.
* **Effet** : Confère +5% de défense (`defense`) et +5% de chances de coup critique (`critChance`) par monstre présent dans un rayon de 5 unités, sans limite de cumul.

### Fonctionnement du Deck de Runes (Sélection active)
Pour éviter la complexité de faire défiler les 15 runes en plein combat, le système repose sur un concept de **Deck Actif** :
* **Hors Combat (Menu/Inventaire)** : Le joueur choisit et équipe jusqu'à **3 runes** maximum parmi celles qu'il a débloquées. Ce sont ses runes équipées.
* **En Combat (HUD / Touche rapide)** : La touche de changement rapide fait défiler de manière circulaire uniquement les **3 runes équipées**.
* **Progression et Déblocage** : Le joueur commence l'aventure avec 1 seul emplacement de rune équipé. Il débloque le 2ème puis le 3ème emplacement de son deck au cours de sa progression (ex: récompense de boss, quêtes ou objets rares).

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
* **HUD** :
  * **Posture** : Une case épurée affichant l'icône de la posture active.
  * **Runes (Deck)** : Une case principale affichant la rune active, flanquée de deux petits indicateurs visuels en retrait (miniatures semi-transparentes ou icônes de taille réduite) montrant les deux autres runes équipées pour anticiper la rotation.
  * À chaque rotation, la case correspondante s'anime brièvement (léger effet de zoom et impulsion lumineuse).
* **Effet en jeu** : Pas d'effet visuel encombrant sur l'épée, mais une lueur colorée très subtile (2D Light) émane du joueur lors de la frappe en fonction de la posture ou rune active.
