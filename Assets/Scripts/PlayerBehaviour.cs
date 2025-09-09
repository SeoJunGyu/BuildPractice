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

    // Start is called before the first frame update
    void Start()
    {
        // Get access to our Rigidbody component 
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
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