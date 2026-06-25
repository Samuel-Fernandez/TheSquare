using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// Fait défiler automatiquement un ScrollRect pour s'assurer que l'élément
    /// sélectionné par l'EventSystem reste toujours dans le Viewport visible.
    /// Idéal pour la navigation au clavier ou à la manette dans les menus.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectAutoScroll : MonoBehaviour
    {
        private ScrollRect scrollRect;
        private RectTransform viewport;
        private RectTransform content;

        [Header("Paramètres")]
        [Tooltip("Marge de défilement en pixels aux limites haute/basse du Viewport")]
        [SerializeField] private float scrollMargin = 30f;

        [Header("Animation")]
        [Tooltip("Indique si le défilement doit être fluide ou instantané")]
        [SerializeField] private bool smoothScroll = true;
        [Tooltip("Vitesse de défilement automatique fluide")]
        [SerializeField] private float scrollSpeed = 10f;

        private float targetScrollY;
        private bool isAutoScrolling = false;
        private GameObject lastSelected;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            if (scrollRect != null)
            {
                viewport = scrollRect.viewport;
                content = scrollRect.content;
            }
        }

        private void Start()
        {
            if (content != null)
            {
                targetScrollY = content.anchoredPosition.y;
            }
        }

        private void LateUpdate()
        {
            if (scrollRect == null || viewport == null || content == null || EventSystem.current == null)
                return;

            GameObject selected = EventSystem.current.currentSelectedGameObject;

            // Gérer le changement de sélection
            if (selected != lastSelected)
            {
                lastSelected = selected;

                if (selected != null && selected.transform.IsChildOf(content))
                {
                    RectTransform targetRT = selected.GetComponent<RectTransform>();
                    if (targetRT != null)
                    {
                        UpdateScrollPosition(targetRT);
                    }
                }
            }

            // Gérer l'animation du défilement
            if (isAutoScrolling)
            {
                // Interrompre le défilement automatique si l'utilisateur utilise activement la souris
                // (clic ou molette pour faire défiler manuellement)
                if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.01f || Input.GetMouseButton(0))
                {
                    isAutoScrolling = false;
                    return;
                }

                Vector2 pos = content.anchoredPosition;
                if (smoothScroll)
                {
                    pos.y = Mathf.Lerp(pos.y, targetScrollY, Time.unscaledDeltaTime * scrollSpeed);
                    if (Mathf.Abs(pos.y - targetScrollY) < 0.1f)
                    {
                        pos.y = targetScrollY;
                        isAutoScrolling = false;
                    }
                }
                else
                {
                    pos.y = targetScrollY;
                    isAutoScrolling = false;
                }
                content.anchoredPosition = pos;
            }
        }

        private void UpdateScrollPosition(RectTransform target)
        {
            // Forcer la mise à jour des layouts pour s'assurer que les positions du content et de la cible sont exactes à cette frame
            Canvas.ForceUpdateCanvases();

            RectTransform activeViewport = viewport != null ? viewport : scrollRect.GetComponent<RectTransform>();

            // Coins mondiaux du viewport
            Vector3[] viewportCorners = new Vector3[4];
            activeViewport.GetWorldCorners(viewportCorners);

            // Convertir les coins mondiaux du viewport dans l'espace local du content
            float viewportMinYInContent = content.InverseTransformPoint(viewportCorners[0]).y; // Bas du viewport dans le content
            float viewportMaxYInContent = content.InverseTransformPoint(viewportCorners[2]).y; // Haut du viewport dans le content

            // Définir les limites de visibilité avec la marge dans le content
            float minYBound = viewportMinYInContent + scrollMargin;
            float maxYBound = viewportMaxYInContent - scrollMargin;

            // Récupérer la position locale de la cible dans le content
            Vector3 targetLocalPos = content.InverseTransformPoint(target.position);
            float buttonHeight = target.rect.height;

            // Calculer le haut et le bas du bouton dans le content en gérant correctement son pivot
            float buttonTop = targetLocalPos.y + (1.0f - target.pivot.y) * buttonHeight;
            float buttonBottom = targetLocalPos.y - target.pivot.y * buttonHeight;

            float diff = 0f;

            // Si le bas de l'élément dépasse vers le bas
            if (buttonBottom < minYBound)
            {
                diff = buttonBottom - minYBound; // Valeur négative
            }
            // Si le haut de l'élément dépasse vers le haut
            else if (buttonTop > maxYBound)
            {
                diff = buttonTop - maxYBound; // Valeur positive
            }

            // Si un ajustement de position est nécessaire
            if (Mathf.Abs(diff) > 0.001f)
            {
                float currentY = content.anchoredPosition.y;
                float viewportHeight = activeViewport.rect.height;
                float maxScrollY = Mathf.Max(0f, content.rect.height - viewportHeight);

                targetScrollY = Mathf.Clamp(currentY - diff, 0f, maxScrollY);
                isAutoScrolling = true;
            }
        }
    }
}
