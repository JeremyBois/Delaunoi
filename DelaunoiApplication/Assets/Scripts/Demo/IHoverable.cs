using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;


public class IHoverable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string displayName;
	public GameObject popup;

    [SerializeField]
    private float YShift = 0;

    public void OnPointerEnter(PointerEventData eventData)
    {
        popup.SetActive(true);
        popup.GetComponentInChildren<Text>().text = displayName;

        var trueBack = popup.transform.GetChild(0).GetComponent<RectTransform>();
        var refBack = popup.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();

        Vector3 pos = popup.transform.position;
        pos.y = transform.position.y + YShift;
        popup.transform.position = pos;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        popup.SetActive(false);
    }
}
