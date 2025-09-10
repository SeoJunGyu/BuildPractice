using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoyStick : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public float speed = 5f;

    public RectTransform background; //���̽�ƽ ����
    public RectTransform handle; //���̽�ƽ ������
    private float radius;

    public Vector2 Input { get; private set; }

    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        var touchPosition = eventData.position; //��ũ�� ��ǥ�� �����̴�.
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, touchPosition, eventData.enterEventCamera, out Vector2 position
            ))
        {
            var delta = position;
            //ClampMagnitude : 
            delta = Vector2.ClampMagnitude(delta, radius);
            handle.anchoredPosition = delta;
            Input = delta / radius;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Input = Vector3.zero;
        handle.anchoredPosition = Vector2.zero;
    }

    private void Start()
    {
        radius = background.rect.width * 0.5f;
    }

    private void Update()
    {
        //Debug.Log(Input);

    }
}
