using UnityEngine;

/// <summary> 
/// Responsible for moving the player automatically and 
/// reciving input. 
/// </summary> 
[RequireComponent(typeof(Rigidbody))]
public class PlayerBehaviour : MonoBehaviour
{
    /// <summary> 
    /// A reference to the Rigidbody component 
    /// </summary> 
    private Rigidbody rb;

    [Tooltip("How fast the ball moves left/right")]
    public float dodgeSpeed = 5;

    [Tooltip("How fast the ball moves forwards automatically")]
    [Range(0, 10)]
    public float rollSpeed = 5;

    //스와이프 필요 멤버들
    //DPI : 디스플레이별 인치 변환
    public float minSwipeDistance = 0.25f; //Inch //이동하는 물리적인 거리
    public float maxSwipeTime = 0.25f; //0.25초가 지나면 스와이프로 판단한다.

    private float minSwipeDistancePixels; //인치를 픽셀로 변환한 값 저장

    private int fingerId = -1; //스와이프는 같은 손가락에서 되어야한다. / 다른 손가락이면 다른 기능이 되어야한다.
    private Vector2 fingerTouchStartPosition; //손가락 시작 지점
    private float fingerTouchStartTime; //손가락 시작 시간

    public float sweepDistance = 1f;

    private float minScale = 0.3f;
    private float maxScale = 2f;
    private float scaleRate = 1f;
    public float zoomSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // Get access to our Rigidbody component 
        rb = GetComponent<Rigidbody>();

        minSwipeDistancePixels = minSwipeDistance * Screen.dpi; //디바이스에서 인치가 몇 픽셀인지 계산된다.
    }

    private void Update()
    {
        //모든 터치 계속 순회
        foreach(Touch touch in Input.touches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (fingerId == -1) //터치가 없는 상태
                    {
                        fingerId = touch.fingerId; //현재 터치 Id 저장

                        fingerTouchStartPosition = touch.position; //터치 시작 위치
                        fingerTouchStartTime = Time.time; //터치 시작 시간
                    }
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if(fingerId == touch.fingerId) //현재 터치와 땠을때 같다면 리셋한다.
                    {
                        float distance = Vector2.Distance(touch.position, fingerTouchStartPosition); //땐 터치 위치와 시작 위치 거리 반환
                        float time = Time.time - fingerTouchStartTime; //현재 시간과 시작시간의 차이

                        if(distance > minSwipeDistancePixels && time < maxSwipeTime) //방향은 모르겠지만 스와이프 조건에 만족했다.
                        {
                            Vector2 direction = touch.position - fingerTouchStartPosition; //방향구하기
                            direction.Normalize();

                            Vector3 direction3 = direction.x > 0f ? Vector3.right : Vector3.left;
                            if(!rb.SweepTest(direction3, out RaycastHit hit, sweepDistance)) //리지드바디가 있는 콜라이더의 범위만큼의 레이를 충돌검사하는 함수다.
                            {
                                rb.MovePosition(rb.position + direction3 * sweepDistance);
                            }
                            
                        }

                        fingerId = -1;
                        fingerTouchStartPosition = Vector2.zero;
                        fingerTouchStartTime = 0f;
                    }
                    break;
            }
        }

        if(Input.touchCount == 2)
        {
            Touch touch = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            var currentDistance = Vector3.Distance(touch.position, touch2.position);
            var prevDistance = Vector3.Distance(touch.position - touch.deltaPosition, touch2.position - touch2.deltaPosition); //현재 포지션과 델타 포지션의 차 : 이전 프레임의 포지션이다.

            float delta = (currentDistance - prevDistance) / Screen.dpi; //현재와 이전 프레임의 거리 변화량 / 픽셀 좌표이니 dpi를 나누면 인치가 나온다. (반대로 인치를 픽셀로 바꿀때는 dpi를 곱한다.)
            delta *= Time.deltaTime * zoomSpeed; //점점 커지도록

            var current = transform.localScale.x;
            current += delta;

            current = Mathf.Clamp(current, minScale, maxScale);
            transform.localScale = Vector3.one * current; //물리적으로 커지기 때문에 싱크가 안맞을 수 있다.
        }

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            var ray = Camera.main.ScreenPointToRay(touch.position);
            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore)) //~0 : 0의 보수니까 전부 1인 레이어만 찾겠다는 것이다.
            {
                //hit.collider.gameObject.SetActive(false);
                //SendMessageOptions.DontRequireReceiver : 
                hit.collider.SendMessage("OnTouched", SendMessageOptions.DontRequireReceiver); //SendMessage : 레이를 맞은 오브젝트의 모든 컴포넌트를 순회하면서 매개변수로 받은 문자열의 이름인 메서드를 전부 호출하는 함수다. / 하나도 없으면 에러난다.
                //SendMessage 장점 : 구조적으로 다른 클래스 호출을 줄일 수 있다. / 단점 ; 찾는데 오래 걸린다. 성능이 안좋다.
            }
        }
    }

    /// <summary>
    /// FixedUpdate is called at a fixed framerate and is a prime place to put
    /// Anything based on time.
    /// </summary>
    void FixedUpdate()
    {
#if UNITY_EDITOR
        var horizontalSpeed = Input.GetAxis("Horizontal") * dodgeSpeed;
#endif

#if UNITY_ANDROID || UNITY_IOS
        //acceleration : 모바일 가속도 센서(기울이기)를 감지해서 값을 반환하는 프로퍼티다.
        //horizontalSpeed = Input.acceleration.x * dodgeSpeed;

        //touchCount : 터치한 손가락 갯수
        if (Input.touchCount == 1)
        {
            //Input.touchCount[0] = 0;
            Touch touch = Input.GetTouch(0);
            var viewportPos = Camera.main.ScreenToViewportPoint(touch.position); ////touch.position : 터치된 좌표
            horizontalSpeed = viewportPos.x < 0.5 ? -1f : 1f;
            horizontalSpeed *= dodgeSpeed;
        }
#endif

        rb.AddForce(horizontalSpeed, 0, rollSpeed);
    }
}