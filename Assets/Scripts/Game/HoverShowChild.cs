using UnityEngine;
using UnityEngine.EventSystems;

public class HoverShowChild : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("需要在此UI元素Hover时显示的子物体")]
    public GameObject targetChild;

    private void Start()
    {
        // 初始状态下隐藏子物体
        if (targetChild != null)
        {
            targetChild.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标悬停时显示子物体
        if (targetChild != null)
        {
            targetChild.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标移出时关闭显示
        if (targetChild != null)
        {
            targetChild.SetActive(false);
        }
    }
}