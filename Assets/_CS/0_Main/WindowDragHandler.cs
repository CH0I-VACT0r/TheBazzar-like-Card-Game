using UnityEngine;
using UnityEngine.UIElements;

public class WindowDragHandler : PointerManipulator
{
    private bool m_IsDragging = false;
    private Vector2 m_StartMousePosition;
    private Vector2 m_StartElementPosition;
    private VisualElement m_WindowToMove; // 실제로 움직일 창 (InventoryRoot)

    // 생성자: target은 "손잡이(Header)", window는 "움직일 몸체"
    public WindowDragHandler(VisualElement targetHandle, VisualElement windowToMove)
    {
        this.target = targetHandle;
        this.m_WindowToMove = windowToMove;
        activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(OnPointerDown);
        target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        target.RegisterCallback<PointerUpEvent>(OnPointerUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
        target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (m_IsDragging) return;

        m_IsDragging = true;
        target.CapturePointer(evt.pointerId);
        m_StartMousePosition = evt.position; // 시작 마우스 좌표

        Vector2 worldPosition = m_WindowToMove.worldBound.position;  // 현재 창의 실제 화면상 위치
        Vector2 localPosition = m_WindowToMove.parent.WorldToLocal(worldPosition); // 부모 기준 로컬 좌표로 변환

        // 변환된 좌표
        m_WindowToMove.style.left = localPosition.x;
        m_WindowToMove.style.top = localPosition.y;
        m_WindowToMove.style.translate = new Translate(0, 0, 0);

        // 시작할 때의 요소 위치
        m_StartElementPosition = new Vector2(localPosition.x, localPosition.y);

        evt.StopPropagation();
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        // 이동량 계산
        Vector2 delta = evt.position - (Vector3)m_StartMousePosition;

        // 새 위치 적용
        m_WindowToMove.style.left = m_StartElementPosition.x + delta.x;
        m_WindowToMove.style.top = m_StartElementPosition.y + delta.y;

        evt.StopPropagation();
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!m_IsDragging || !target.HasPointerCapture(evt.pointerId)) return;

        m_IsDragging = false;
        target.ReleasePointer(evt.pointerId);
        evt.StopPropagation();
    }
}
