# Bestiaire du Jeu

### 1. Gargouille de Pierre
- **Comportement** : Camouflée en statue de décor (parfaitement immobile et invulnérable).
- **Attaque** : S'anime subitement et fond sur le joueur seulement lorsque celui-ci lui tourne le dos ou tente d'activer un bouton mécanique. Retourne à son état de statue de pierre après son attaque. Idéale pour les salles d'énigmes.

### 2. Prêtre Corrompu (Soutien)
- **Comportement** : Support stratégique qui reste toujours à distance maximale du joueur grâce à la condition (`target - position` inversée).
- **Mécanique** : Ne fait aucun dégât, mais canalise un rayon qui empêche un autre monstre de la salle de mourir ou qui lui donne un boost de vitesse significatif (`speedMultiplier` poussé à x2). Il force le joueur à revoir ses priorités de ciblage.