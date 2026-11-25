using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_Match_Scroll_Wheel_To_Selected_Button : MonoBehaviour
{
    [SerializeField] GameObject currentSelected;
    [SerializeField] GameObject previouslySelected;
    [SerializeField] RectTransform currentSelectedTransform;

    [SerializeField] RectTransform contentPanel;
    [SerializeField] ScrollRect scrollRect;

    private void Update()
    {
        var selected = EventSystem.current?.currentSelectedGameObject;

        if (selected == null || !selected.transform.IsChildOf(contentPanel))
        {
            previouslySelected = null;
            return;
        }

        if (selected == previouslySelected)
            return;

        previouslySelected = selected;

        if (selected.transform is RectTransform target)
            SnapTo(target);
    }

    private void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        var viewport = scrollRect.viewport != null
            ? scrollRect.viewport
            : (RectTransform)scrollRect.transform;

        var targetBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, target);
        var contentBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, contentPanel);

        float scrollableHeight = contentBounds.size.y - viewport.rect.height;
        if (scrollableHeight <= 0f)
            return;

        float offset = 0f;

        if (targetBounds.max.y > viewport.rect.yMax)
            offset = viewport.rect.yMax - targetBounds.max.y;
        else if (targetBounds.min.y < viewport.rect.yMin)
            offset = viewport.rect.yMin - targetBounds.min.y;

        if (Mathf.Abs(offset) < 0.01f)
            return;

        scrollRect.StopMovement();
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
            scrollRect.verticalNormalizedPosition - offset / scrollableHeight);
    }
}
