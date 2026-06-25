using System.Collections.Generic;
using UnityEngine;

namespace UI.Interfaces.Crafting
{
    /// <summary>
    /// Gère la disposition et le tracé de connexions pour un arbre de crafting sous forme de grille.
    /// Élimine tout espace vide inutile et utilise un prefab unique pour tous les nœuds.
    /// Les exigences d'équipement sont générées automatiquement en fonction des connexions.
    /// </summary>
    public class CraftingGridFiller : MonoBehaviour
    {
        [System.Serializable]
        public struct GridCell
        {
            public Item item;            // L'item produit par ce nœud
            public string nodeName;      // Nom du nœud (affiché dans l'éditeur)
            public bool isActive;        // Si coché, le nœud existe
            public List<SpecialItemRequirement> specialItemsRequired; // Ingrédients de type SpecialItems requis
        }

        [System.Serializable]
        public struct GridConnection
        {
            public int parentFlatIndex; // Index à plat de la cellule parente
            public int childFlatIndex;  // Index à plat de la cellule enfant
        }

        [Header("Taille de la Grille")]
        [Range(1, 15)][SerializeField] private int rows = 5;
        [Range(1, 15)][SerializeField] private int columns = 7;

        // Stockage de la grille
        [HideInInspector][SerializeField] private GridCell[] gridCells = new GridCell[0];

        [Header("Structure UI")]
        [SerializeField] private RectTransform scrollViewport; // Le Viewport du ScrollView
        [SerializeField] private RectTransform contentPanel;      // Le conteneur "Content"
        [SerializeField] private GameObject nodePrefab;           // Prefab unique de slot pour tous les nœuds
        [SerializeField] private RectTransform linePrefab;        // Prefab d'image pour les lignes de connexion
        [SerializeField] private ItemDescriptionPanel itemDescriptionPanel; // Panel de description d'item

        public static CraftingGridFiller instance;
        private CraftingSlot currentSelectedSlot;

        private void Awake()
        {
            instance = this;
        }

        [Header("Connexions")]
        [SerializeField] private List<GridConnection> connections = new List<GridConnection>();

        [Header("Paramètres de mise en page")]
        [SerializeField] private float topPadding = 50f;
        [SerializeField] private float bottomPadding = 50f;
        [SerializeField] private float sideMargin = 60f;
        [SerializeField] private float rowSpacing = 150f;
        [SerializeField] private float lineThickness = 4f;

        // Suivi pour le nettoyage
        private List<GameObject> instantiatedNodes = new List<GameObject>();
        private List<GameObject> instantiatedLines = new List<GameObject>();

        public int Rows => rows;
        public int Columns => columns;
        public List<GridConnection> Connections => connections;

        private void OnValidate()
        {
            ValidateGridSize();
        }

        private void ValidateGridSize()
        {
            int targetSize = rows * columns;
            if (gridCells == null || gridCells.Length != targetSize)
            {
                GridCell[] temp = new GridCell[targetSize];
                if (gridCells != null)
                {
                    for (int i = 0; i < Mathf.Min(gridCells.Length, temp.Length); i++)
                    {
                        temp[i] = gridCells[i];
                    }
                }
                gridCells = temp;
            }
        }

        public GridCell GetCell(int col, int row)
        {
            int index = row * columns + col;
            if (gridCells == null || index < 0 || index >= gridCells.Length) return new GridCell();
            return gridCells[index];
        }

        public void SetCell(int col, int row, GridCell cell)
        {
            int index = row * columns + col;
            ValidateGridSize();
            if (index >= 0 && index < gridCells.Length)
            {
                gridCells[index] = cell;
            }
        }

        private void Start()
        {
            GenerateGrid();
        }

        [ContextMenu("Générer la grille")]
        public void GenerateGrid()
        {
            ClearGrid();
            ValidateGridSize();

            if (contentPanel == null || scrollViewport == null || nodePrefab == null)
            {
                Debug.LogError("CraftingGridFiller : Veuillez assigner les composants requis (Content Panel, Scroll Viewport et Node Prefab) !");
                return;
            }

            // Fixer la largeur du content à celle du viewport (pas de scroll horizontal)
            float viewportWidth = scrollViewport.rect.width;
            if (viewportWidth <= 0)
            {
                viewportWidth = scrollViewport.sizeDelta.x;
            }
            contentPanel.sizeDelta = new Vector2(viewportWidth, contentPanel.sizeDelta.y);

            // 1. Trouver les lignes actives minimale et maximale pour rogner l'espace inutile
            int lowestActiveRow = -1;
            int highestActiveRow = -1;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (GetCell(c, r).isActive)
                    {
                        if (lowestActiveRow == -1 || r < lowestActiveRow) lowestActiveRow = r;
                        if (r > highestActiveRow) highestActiveRow = r;
                    }
                }
            }

            // Si aucun nœud n'est actif, on ne génère rien
            if (lowestActiveRow == -1) return;

            int activeRowsSpan = highestActiveRow - lowestActiveRow;

            float usableWidth = viewportWidth - (sideMargin * 2f);
            float colSpacing = columns > 1 ? (usableWidth / (columns - 1)) : usableWidth;

            // Dictionnaire pour retrouver les RectTransforms créés par leur index à plat
            Dictionary<int, RectTransform> spawnedNodes = new Dictionary<int, RectTransform>();

            // 2. Instancier et positionner uniquement les nœuds actifs
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    GridCell cell = GetCell(c, r);
                    if (!cell.isActive) continue;

                    GameObject nodeInstance = Instantiate(nodePrefab, contentPanel);
                    nodeInstance.name = string.IsNullOrEmpty(cell.nodeName) ? $"Slot_{c}_{r}" : $"Slot_{cell.nodeName}";
                    instantiatedNodes.Add(nodeInstance);

                    RectTransform nodeRT = nodeInstance.GetComponent<RectTransform>();
                    int flatIndex = r * columns + c;
                    spawnedNodes.Add(flatIndex, nodeRT);

                    // Injecter les données de recette configurées
                    CraftingSlot slotComponent = nodeInstance.GetComponent<CraftingSlot>();
                    if (slotComponent != null)
                    {
                        slotComponent.SetupSlot(cell.item);
                        slotComponent.specialItemsRequired = cell.specialItemsRequired != null
                            ? new List<SpecialItemRequirement>(cell.specialItemsRequired)
                            : new List<SpecialItemRequirement>();

                        // Réinitialiser la liste d'équipements requis (elle sera remplie par les connexions ci-dessous)
                        slotComponent.equipmentRequired = new List<EquipmentRequirement>();
                    }

                    // Configuration des ancres au coin supérieur gauche
                    nodeRT.anchorMin = new Vector2(0, 1);
                    nodeRT.anchorMax = new Vector2(0, 1);
                    nodeRT.pivot = new Vector2(0.5f, 0.5f);

                    // Position locale avec rognage (l'ordre suit celui de l'inspecteur)
                    float posX = sideMargin + (c * colSpacing);
                    float posY = -topPadding - ((r - lowestActiveRow) * rowSpacing);

                    nodeRT.anchoredPosition = new Vector2(posX, posY);
                }
            }

            // 3. Dessiner les lignes et lier automatiquement les exigences d'équipement
            foreach (var conn in connections)
            {
                if (spawnedNodes.TryGetValue(conn.parentFlatIndex, out RectTransform parentRT) &&
                    spawnedNodes.TryGetValue(conn.childFlatIndex, out RectTransform childRT))
                {
                    // A. Dessiner la ligne de connexion
                    if (linePrefab != null)
                    {
                        DrawConnectionLine(parentRT, childRT);
                    }

                    // B. Lier l'équipement requis (l'enfant est requis pour crafter le parent)
                    CraftingSlot parentSlot = parentRT.GetComponent<CraftingSlot>();
                    CraftingSlot childSlot = childRT.GetComponent<CraftingSlot>();

                    if (parentSlot != null && childSlot != null && childSlot.itemToCraft != null)
                    {
                        EquipmentRequirement eqReq = new EquipmentRequirement();
                        eqReq.baseItem = childSlot.itemToCraft;
                        eqReq.amount = 1;
                        parentSlot.equipmentRequired.Add(eqReq);
                    }
                }
            }

            // 4. Ajuster la hauteur de défilement exacte de l'arbre rogné
            float totalHeight = topPadding + (activeRowsSpan * rowSpacing) + bottomPadding;
            contentPanel.sizeDelta = new Vector2(viewportWidth, totalHeight);
        }

        private void DrawConnectionLine(RectTransform parentNode, RectTransform childNode)
        {
            GameObject lineGo = Instantiate(linePrefab.gameObject, contentPanel);
            lineGo.name = $"Line_{parentNode.name}_to_{childNode.name}";
            lineGo.transform.SetAsFirstSibling();
            instantiatedLines.Add(lineGo);

            RectTransform lineRT = lineGo.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0, 1);
            lineRT.anchorMax = new Vector2(0, 1);
            lineRT.pivot = new Vector2(0.5f, 0.5f);

            Vector2 posA = parentNode.anchoredPosition;
            Vector2 posB = childNode.anchoredPosition;

            Vector2 direction = posB - posA;
            float distance = direction.magnitude;

            lineRT.anchoredPosition = posA + (direction * 0.5f);
            lineRT.sizeDelta = new Vector2(distance, lineThickness);

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            lineRT.rotation = Quaternion.Euler(0, 0, angle);
        }

        public void ClearGrid()
        {
            foreach (var node in instantiatedNodes)
            {
                if (node != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying) { DestroyImmediate(node); continue; }
#endif
                    Destroy(node);
                }
            }
            instantiatedNodes.Clear();

            foreach (var line in instantiatedLines)
            {
                if (line != null)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying) { DestroyImmediate(line); continue; }
#endif
                    Destroy(line);
                }
            }
            instantiatedLines.Clear();

            // Nettoyage de sécurité
            List<GameObject> oldObjects = new List<GameObject>();
            foreach (Transform child in contentPanel)
            {
                if (child.name.StartsWith("Line_Slot_") || child.name.StartsWith("Slot_"))
                {
                    oldObjects.Add(child.gameObject);
                }
            }
            foreach (var obj in oldObjects)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) { DestroyImmediate(obj); continue; }
#endif
                Destroy(obj);
            }
        }

        /// <summary>
        /// Affiche ou masque le panneau de description de l'item du slot cliqué.
        /// </summary>
        public void ShowItemDescription(CraftingSlot slot)
        {
            Debug.Log($"[CraftingGridFiller] ShowItemDescription appelé pour le slot '{slot.gameObject.name}'.");

            if (itemDescriptionPanel == null)
            {
                Debug.LogWarning("CraftingGridFiller : Le panneau 'ItemDescriptionPanel' n'est pas assigné dans l'inspecteur sur " + gameObject.name + " !");
                return;
            }

            Debug.Log($"[CraftingGridFiller] Panel activeSelf: {itemDescriptionPanel.gameObject.activeSelf}, currentSelectedSlot: {(currentSelectedSlot != null ? currentSelectedSlot.gameObject.name : "None")}");

            if (itemDescriptionPanel.gameObject.activeSelf && currentSelectedSlot == slot)
            {
                Debug.Log("[CraftingGridFiller] Désactivation du panneau car clic sur le même slot.");
                itemDescriptionPanel.gameObject.SetActive(false);
                currentSelectedSlot = null;
            }
            else
            {
                Debug.Log($"[CraftingGridFiller] Activation du panneau et appel de DisplayItem.");
                currentSelectedSlot = slot;
                itemDescriptionPanel.DisplayItem(slot);
            }
        }
    }
}
