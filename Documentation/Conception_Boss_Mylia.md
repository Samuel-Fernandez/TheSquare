# Conception : Boss Mylia, Gardienne de l'Interaction Faible

TODO :
- Détailler le contenu précis de la Phase 1 (EN COURS)
- Détailler le contenu précis des Phases 2 et 3
- Définir la transition Humaine → Gardienne
- Définir le comportement/design de l'arène (mis de côté pour plus tard)

Ce document décrit le design du combat de boss contre **Mylia**, Gardienne de l'Interaction Faible, rencontrée
au Sanctuaire de Mylia (Mirror Lake of Mylia). Voir `Assets/Resources/Localization/en/pnj_text.json` pour le
lore et les dialogues associés (clés `myliaDoor-1`, `BigIronBarrierOpen-1`, `theSquareInteraction-1`, etc.).

## 1. Intention narrative et thématique

Le combat se déroule en **3 phases** : une forme **Humaine** puis deux formes **Gardienne**.

- **Phase 1 (Humaine)** : des attaques de glace, **sans rapport avec son véritable pouvoir**, mais cohérentes
  avec son tempérament — froid, triste, protecteur. Elle retient ses coups.
- **Phases 2 et 3 (Gardienne)** : des attaques directement liées à l'**interaction faible**, sa véritable
  nature en tant que Gardienne.

Le brief thématique central vient de la ligne de dialogue `BigIronBarrierOpen-1` :
> *"Mylia is gentle... Far too gentle... Her attacks seem so insignificant... That's precisely what makes them
> so dangerous."*

Le combat ne doit donc pas se lire comme un boss "brutal" classique : chaque attaque doit sembler presque
anodine, et le danger vient de l'accumulation / de l'interprétation erronée, pas du burst de dégâts.

Le nom du lieu, **Mirror Lake of Mylia**, est également exploité mécaniquement : l'interaction faible est la
seule force fondamentale à **violer la parité** (la symétrie miroir), ce qui inspire directement le mécanisme
de la Phase 2 (voir plus bas).

## 2. Outils du joueur disponibles pendant le combat

Le joueur dispose potentiellement de trois objets obtenus dans le donjon (Sanctuaire de Mylia) :
- **La Lanterne** : permet d'enflammer des objets, ou d'éclairer/voir dans l'obscurité. **Utilisée dans les
  3 phases.**
- **L'Arc** : utilisé spécifiquement en **Phase 2**, couplé à la Lanterne.
- **La Pioche** : disponible mais pas indispensable au combat (rôle non défini pour l'instant).

## 3. Phase 1 — Humaine (Glace)

- Thème : chagrin, retenue, tempérament froid — **sans rapport avec son véritable pouvoir** (l'interaction
  faible), purement lié à sa personnalité.
- Attaques larges et bien télégraphiées, dégâts individuels faibles : elle ne se bat pas réellement, elle se
  contient.
- Elle plonge la zone dans l'obscurité par moments ; la **Lanterne** sert à éclairer/révéler les attaques ou
  telegraphs qui seraient sinon invisibles.
- Les 3 attaques ci-dessous reviennent en **cycle aléatoire** (intervalle non prévisible entre chaque, sur le
  modèle de `BigIceBumperBehiavor.AttackCycleLoop`) : le joueur ne peut pas mémoriser une séquence fixe, il
  doit rester attentif en continu.

### 3.1. Attaque 1 — Glaçon fonceur

Elle assombrit la scène, s'enferme dans un bloc de glace, le sol de la scène devient glacé (glissant), et elle
fonce dans les murs de l'arène en rebondissant — inspiré directement du comportement de
`Assets/Scripts/Game/Entities/LivingEntity/Monster/BigIceBumper/BigIceBumperBehiavor.cs` (tournoiement à
vitesse multipliée, rebond sur les murs via réflexion de la normale de collision, contact avec le joueur
infligeant des dégâts).

**Fenêtre de dégâts n°1** : une fois qu'elle s'arrête après avoir foncé dans les murs, le joueur peut brûler
son bloc de glace avec la **Lanterne**, puis la frapper à l'épée pendant qu'elle est ainsi exposée.

### 3.2. Attaque 2 — Stalactites

Elle s'envole et fait tomber des stalactites du plafond sur l'arène. Certaines, une fois détruites/brisées,
libèrent des objets utiles (cœurs de soin, flèches, etc.) — une attaque à éviter qui récompense aussi la prise
de risque.

### 3.3. Attaque 3 — Pics de glace renvoyables

Elle s'envole et lance des pics de glace en direction du joueur. En les frappant avec le bon timing (parade),
le joueur les renvoie vers Mylia, lui infligeant des dégâts et la faisant tomber au sol.

**Fenêtre de dégâts n°2** : un renvoi réussi la rend **vulnérable pendant 4 à 5 secondes**, pendant lesquelles
le joueur peut la frapper librement.

## 4. Phase 2 — Gardienne I (Violation de parité + changement de saveur)

- **Mécanisme des telegraphs mensongers** : l'indicateur/l'animation d'attaque annonce une direction, mais
  l'attaque réelle part en miroir de ce qui est annoncé (violation de parité). Demande une mémorisation et un
  apprentissage plutôt qu'une lecture réflexe.
- **Cycle Lanterne + Arc** :
  1. Des objets inflammables sont dispersés dans l'arène.
  2. Le joueur doit **tous** les enflammer avec la Lanterne en esquivant les attaques de Mylia (dont les
     telegraphs mensongers).
  3. Une fois tous les objets brûlés, Mylia devient **vulnérable**.
  4. Le joueur doit alors la toucher avec l'**Arc**. Si elle n'est **pas** vulnérable, la flèche tirée se
     transforme en un **papillon** temporaire et inoffensif au lieu de la toucher (représentation ludique du
     changement de saveur — *flavor change* — propre à l'interaction faible : l'attaque change de nature au
     contact plutôt que d'infliger des dégâts).

## 5. Phase 3 — Gardienne II

- La **Lanterne** reste utile pour éclairer/voir dans l'obscurité, comme en Phase 1 (probablement intensifié).
- Un cycle de vulnérabilité similaire à la Phase 2 est présent (brûler les objets inflammables de l'arène),
  mais **sans l'Arc** : une fois Mylia vulnérable, le joueur revient à l'**épée en mêlée classique**. Le
  rapprochement nécessaire au corps-à-corps (contre une attaque à distance sûre en Phase 2) fait monter la
  tension même sans nouveau gadget.
- Reste à définir : une attaque signature propre à cette phase pour la distinguer davantage de la Phase 2 et
  incarner le "danger insignifiant mais mortel" de l'interaction faible sous un autre angle que le retour à
  la mêlée seul.

## 6. Points ouverts / à trancher

1. Attaque signature de la Phase 3 différenciant clairement les deux formes Gardienne.
2. Détail du cycle "brûler les objets" : nombre d'objets, méthode d'allumage (contact direct avec la Lanterne ?
   projection de feu ?), comportement en cas de reset (le joueur se fait toucher pendant le cycle — est-ce que
   la progression est perdue ?).
3. Transition Humaine → Gardienne : déclencheur, mise en scène, ce que ça révèle du personnage.
4. Design de l'arène (mis de côté pour plus tard, y compris son évolution visuelle éventuelle entre les
   phases).
