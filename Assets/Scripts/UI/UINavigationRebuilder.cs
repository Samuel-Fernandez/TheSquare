using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Reconstruit automatiquement la navigation explicite entre tous les boutons
    /// enfants actifs de ce GameObject (Canvas, Panel, etc.).
    ///
    /// Modes disponibles :
    ///   - Automatic : détecte la disposition (grille, ligne, colonne) et construit
    ///                 la navigation la plus naturelle possible.
    ///   - Vertical   : chaîne les boutons uniquement haut/bas dans l'ordre Y.
    ///   - Horizontal : chaîne les boutons uniquement gauche/droite dans l'ordre X.
    ///   - Grid       : construit une grille basée sur le nombre de colonnes configuré.
    ///
    /// Pose ce composant sur le Canvas ou le Panel parent. La navigation est
    /// reconstruite à l'activation de l'objet et peut être déclenchée manuellement
    /// via RebuildNavigation().
    /// </summary>
    public class UINavigationRebuilder : MonoBehaviour
    {
        // ──────────────────────────────────────────────────────────────────────────
        // Types & constantes
        // ──────────────────────────────────────────────────────────────────────────

        public enum NavigationMode
        {
            /// <summary>
            /// Détecte automatiquement la disposition et choisit le meilleur mode.
            /// </summary>
            Automatic,
            /// <summary>
            /// Navigation verticale uniquement (haut / bas).
            /// </summary>
            Vertical,
            /// <summary>
            /// Navigation horizontale uniquement (gauche / droite).
            /// </summary>
            Horizontal,
            /// <summary>
            /// Grille avec un nombre de colonnes défini manuellement.
            /// </summary>
            Grid
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Champs sérialisés
        // ──────────────────────────────────────────────────────────────────────────

        [Header("Mode de navigation")]
        [Tooltip("Choisir comment les boutons sont chaînés entre eux.")]
        [SerializeField] private NavigationMode mode = NavigationMode.Automatic;

        [Header("Options Grille")]
        [Tooltip("Nombre de colonnes (uniquement pour le mode Grid).")]
        [SerializeField] private int gridColumns = 2;

        [Header("Boucle de navigation")]
        [Tooltip("En mode Vertical / Automatic vertical : le dernier bouton pointe vers le premier (et vice-versa).")]
        [SerializeField] private bool wrapVertical = false;
        [Tooltip("En mode Horizontal / Grid : le dernier bouton d'une ligne pointe vers le premier (et vice-versa).")]
        [SerializeField] private bool wrapHorizontal = false;

        [Header("Reconstruction automatique")]
        [Tooltip("Reconstruire la navigation dès que l'objet est activé.")]
        [SerializeField] private bool rebuildOnEnable = true;
        [Tooltip("Tolérance en pixels pour regrouper des boutons sur la même ligne (mode Automatic / Grid).")]
        [SerializeField] private float rowGroupingTolerance = 10f;

        // ──────────────────────────────────────────────────────────────────────────
        // Cycle de vie Unity
        // ──────────────────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            if (rebuildOnEnable)
                RebuildNavigation();
        }

        // ──────────────────────────────────────────────────────────────────────────
        // API publique
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Reconstruit la navigation explicite de tous les boutons enfants actifs.
        /// Peut être appelé depuis n'importe quel autre script à tout moment.
        /// </summary>
        public void RebuildNavigation()
        {
            List<Button> buttons = CollectActiveButtons();
            if (buttons.Count == 0) return;

            switch (mode)
            {
                case NavigationMode.Vertical:
                    BuildVertical(buttons);
                    break;
                case NavigationMode.Horizontal:
                    BuildHorizontal(buttons);
                    break;
                case NavigationMode.Grid:
                    BuildGrid(buttons, gridColumns);
                    break;
                case NavigationMode.Automatic:
                default:
                    BuildAutomatic(buttons);
                    break;
            }
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Collecte des boutons
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Retourne tous les Button enfants actifs et interactables, triés de haut
        /// en bas puis de gauche à droite (ordre naturel de lecture).
        /// </summary>
        private List<Button> CollectActiveButtons()
        {
            Button[] allButtons = GetComponentsInChildren<Button>(false); // false = actifs seulement
            List<Button> result = new List<Button>();

            foreach (Button btn in allButtons)
            {
                if (btn.interactable && btn.gameObject.activeInHierarchy)
                    result.Add(btn);
            }

            // Tri : Y décroissant (haut d'abord), puis X croissant (gauche d'abord)
            result.Sort((a, b) =>
            {
                Vector2 posA = GetWorldPos(a);
                Vector2 posB = GetWorldPos(b);

                float dy = posB.y - posA.y;
                if (Mathf.Abs(dy) > rowGroupingTolerance) return dy > 0 ? 1 : -1;
                return posA.x.CompareTo(posB.x);
            });

            return result;
        }

        private static Vector2 GetWorldPos(Button btn)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();
            return rt != null ? (Vector2)rt.position : (Vector2)btn.transform.position;
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Modes de construction
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Mode Vertical : chaîne les boutons haut / bas.
        /// </summary>
        private void BuildVertical(List<Button> buttons)
        {
            int count = buttons.Count;
            for (int i = 0; i < count; i++)
            {
                Navigation nav = CreateBaseNavigation();

                // Haut
                int prevIdx = i - 1;
                if (prevIdx < 0) prevIdx = wrapVertical ? count - 1 : -1;
                nav.selectOnUp = prevIdx >= 0 ? buttons[prevIdx] : null;

                // Bas
                int nextIdx = i + 1;
                if (nextIdx >= count) nextIdx = wrapVertical ? 0 : -1;
                nav.selectOnDown = nextIdx >= 0 ? buttons[nextIdx] : null;

                buttons[i].navigation = nav;
            }
        }

        /// <summary>
        /// Mode Horizontal : chaîne les boutons gauche / droite.
        /// </summary>
        private void BuildHorizontal(List<Button> buttons)
        {
            int count = buttons.Count;
            for (int i = 0; i < count; i++)
            {
                Navigation nav = CreateBaseNavigation();

                // Gauche
                int prevIdx = i - 1;
                if (prevIdx < 0) prevIdx = wrapHorizontal ? count - 1 : -1;
                nav.selectOnLeft = prevIdx >= 0 ? buttons[prevIdx] : null;

                // Droite
                int nextIdx = i + 1;
                if (nextIdx >= count) nextIdx = wrapHorizontal ? 0 : -1;
                nav.selectOnRight = nextIdx >= 0 ? buttons[nextIdx] : null;

                buttons[i].navigation = nav;
            }
        }

        /// <summary>
        /// Mode Grid : grille avec <paramref name="columns"/> colonnes.
        /// Navigation haut/bas entre lignes, gauche/droite sur la même ligne.
        /// </summary>
        private void BuildGrid(List<Button> buttons, int columns)
        {
            if (columns <= 0) columns = 1;
            int count = buttons.Count;

            for (int i = 0; i < count; i++)
            {
                int row    = i / columns;
                int col    = i % columns;
                int rows   = Mathf.CeilToInt((float)count / columns);

                Navigation nav = CreateBaseNavigation();

                // Gauche
                if (col > 0)
                {
                    nav.selectOnLeft = buttons[i - 1];
                }
                else if (wrapHorizontal)
                {
                    int wrapIdx = i + columns - 1;
                    nav.selectOnLeft = wrapIdx < count ? buttons[wrapIdx] : null;
                }

                // Droite
                if (col < columns - 1 && i + 1 < count)
                {
                    nav.selectOnRight = buttons[i + 1];
                }
                else if (wrapHorizontal && col == columns - 1)
                {
                    nav.selectOnRight = buttons[row * columns];
                }

                // Haut
                int upIdx = i - columns;
                if (upIdx >= 0)
                {
                    nav.selectOnUp = buttons[upIdx];
                }
                else if (wrapVertical)
                {
                    int wrapUpRow = rows - 1;
                    int wrapUpIdx = wrapUpRow * columns + col;
                    if (wrapUpIdx >= count) wrapUpIdx = count - 1;
                    nav.selectOnUp = wrapUpIdx != i ? buttons[wrapUpIdx] : null;
                }

                // Bas
                int downIdx = i + columns;
                if (downIdx < count)
                {
                    nav.selectOnDown = buttons[downIdx];
                }
                else if (wrapVertical)
                {
                    int wrapDownIdx = col;
                    nav.selectOnDown = wrapDownIdx != i ? buttons[wrapDownIdx] : null;
                }

                buttons[i].navigation = nav;
            }
        }

        /// <summary>
        /// Mode Automatic : regroupe les boutons par lignes (en fonction de leur
        /// position Y) puis choisit le mode le plus adapté :
        ///   - 1 ligne  → Horizontal
        ///   - 1 colonne → Vertical
        ///   - plusieurs lignes de longueurs variables → Grille adaptative
        /// </summary>
        private void BuildAutomatic(List<Button> buttons)
        {
            List<List<Button>> rows = GroupIntoRows(buttons);

            bool singleRow    = rows.Count == 1;
            bool singleColumn = rows.Count == buttons.Count; // chaque ligne n'a qu'un bouton

            if (singleRow)
            {
                BuildHorizontal(buttons);
                return;
            }

            if (singleColumn)
            {
                BuildVertical(buttons);
                return;
            }

            // Grille adaptative : navigation entre lignes de tailles variables
            BuildAdaptiveGrid(rows);
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Grille adaptative (lignes de tailles variables)
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Construit une navigation en grille sur des lignes de tailles potentiellement
        /// différentes. Chaque bouton cherche son voisin le plus proche en X dans la
        /// ligne adjacente.
        /// </summary>
        private void BuildAdaptiveGrid(List<List<Button>> rows)
        {
            int rowCount = rows.Count;

            for (int r = 0; r < rowCount; r++)
            {
                List<Button> row = rows[r];
                int colCount = row.Count;

                for (int c = 0; c < colCount; c++)
                {
                    Button btn = row[c];
                    Navigation nav = CreateBaseNavigation();

                    // ── Gauche / Droite sur la même ligne ──────────────────────
                    if (c > 0)
                        nav.selectOnLeft = row[c - 1];
                    else if (wrapHorizontal)
                        nav.selectOnLeft = row[colCount - 1];

                    if (c < colCount - 1)
                        nav.selectOnRight = row[c + 1];
                    else if (wrapHorizontal)
                        nav.selectOnRight = row[0];

                    // ── Haut : ligne précédente, bouton le plus proche en X ───
                    if (r > 0)
                        nav.selectOnUp = FindClosestInRow(btn, rows[r - 1]);
                    else if (wrapVertical)
                        nav.selectOnUp = FindClosestInRow(btn, rows[rowCount - 1]);

                    // ── Bas : ligne suivante, bouton le plus proche en X ──────
                    if (r < rowCount - 1)
                        nav.selectOnDown = FindClosestInRow(btn, rows[r + 1]);
                    else if (wrapVertical)
                        nav.selectOnDown = FindClosestInRow(btn, rows[0]);

                    btn.navigation = nav;
                }
            }
        }

        /// <summary>
        /// Parmi les boutons d'une ligne, retourne celui dont la position X est la
        /// plus proche du bouton de référence.
        /// </summary>
        private static Button FindClosestInRow(Button reference, List<Button> row)
        {
            if (row == null || row.Count == 0) return null;

            float refX   = GetWorldPos(reference).x;
            Button best  = null;
            float bestDist = float.MaxValue;

            foreach (Button candidate in row)
            {
                float dist = Mathf.Abs(GetWorldPos(candidate).x - refX);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = candidate;
                }
            }
            return best;
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Regroupement par lignes
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Regroupe une liste de boutons (déjà triés Y desc, X asc) en lignes.
        /// Deux boutons sont sur la même ligne si leur Y diffère de moins de
        /// <see cref="rowGroupingTolerance"/> pixels.
        /// </summary>
        private List<List<Button>> GroupIntoRows(List<Button> buttons)
        {
            List<List<Button>> rows = new List<List<Button>>();
            if (buttons.Count == 0) return rows;

            List<Button> currentRow = new List<Button> { buttons[0] };
            float rowY = GetWorldPos(buttons[0]).y;

            for (int i = 1; i < buttons.Count; i++)
            {
                float y = GetWorldPos(buttons[i]).y;
                if (Mathf.Abs(y - rowY) <= rowGroupingTolerance)
                {
                    currentRow.Add(buttons[i]);
                }
                else
                {
                    rows.Add(currentRow);
                    currentRow = new List<Button> { buttons[i] };
                    rowY = y;
                }
            }
            rows.Add(currentRow);
            return rows;
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Utilitaires
        // ──────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Crée une Navigation en mode Explicit avec toutes les directions à null.
        /// </summary>
        private static Navigation CreateBaseNavigation()
        {
            return new Navigation { mode = Navigation.Mode.Explicit };
        }
    }
}
