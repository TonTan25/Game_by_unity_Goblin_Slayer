using UnityEngine;
public class BPCR_BasicPlayerControlRigid : MonoBehaviour {
    // di chuyển người chơi cơ bản
    // có thể nhảy nhiều lần, không thể ngồi
    // add physics material, set dynamic, static to 0 and friction to minimum
    // also assign physic material
    //player input
    public Camera cams; // lấy camera
    [SerializeField] float CamsLerpTime, RunFOV; // thời gian phóng màn hình
    // cơ chế chạy
    //player control adjust
    public float BaseSpeed = 300f, CurrSpeed, WalkSpeed, RunSpeed, Acceleration = 10f;
    //            tốc độ cơ bản  | tốc độ hiện tại| đi |  chạy   | tốc độ chuyển lại giữa đi và chạy
    [Range(1.2f, 3f)] public float RunMulitple = 1.5f; // điều chỉnh tốc độ khi chạy nhanh
    //jump
    public float jumpforce = 5f, jumptime = 2, jumpleft; // lực nhảy - đề xuất 5
    //                lực nhảy | số lần nhảy | số lần nhảy còn lại
    [HideInInspector] public Rigidbody rbc;
    Vector3 PlayerMovementInput, PlayerScale;
    [HideInInspector]public bool jumpable = true, canRun = true; // cho phép nhảy
    float CamsNormalFov, x, y; // góc nhìn bình thường
    void Start(){
        rbc = GetComponent<Rigidbody>();
        // cài đặt di chuyển và chạy
        SpeedReFresh(BaseSpeed);
        // camera khi chạy
        CamsNormalFov = cams.fieldOfView;
        RunFOV += cams.fieldOfView;
        // cài đặt rigid
        rbc.interpolation = RigidbodyInterpolation.Interpolate; // khiến camera và những thứ khác chạy mượt hơn
        rbc.freezeRotation = true; // khóa góc xoay
        PlayerScale = transform.localScale; // gì đây ?
        Cursor.lockState = CursorLockMode.Locked; // khóa chuột vào giữa màn hình
        Cursor.visible = false; // ẩn con chuột khi
    }
    public void SpeedReFresh(float NewSpeedSet){
        WalkSpeed = NewSpeedSet;
        RunSpeed = NewSpeedSet * RunMulitple;
        CurrSpeed = NewSpeedSet;
    }
    void LateUpdate(){
        jumping();
        SpeedControl();
    }
    void FixedUpdate(){
        moving();
    }
    void moving(){
        rbc.AddForce(Vector3.down * Time.deltaTime * 5); // 1 lực nhẹ kéo xuống để cho chắc chắn tiếp đất
        // lấy số đầu vào khi bấm di chuyển
        PlayerMovementInput = new Vector3(Input.GetAxis("Horizontal"), PlayerMovementInput.y, Input.GetAxis("Vertical"));    
        // sửa lại hướng và tốc độ di chuyển
        Vector3 movevector = transform.TransformDirection(PlayerMovementInput.normalized * CurrSpeed * Time.deltaTime);
        rbc.velocity = new Vector3(movevector.x, rbc.velocity.y, movevector.z); // bắt đầu di chuyển
    }

    void SpeedControl(){ // đi và chạy
        if (canRun && Input.GetKey(KeyCode.LeftShift)){ // chạy
            CurrSpeed = Mathf.Lerp(WalkSpeed, RunSpeed, Acceleration * Time.deltaTime); 
            // hiệu ứng camera khi chạy
            cams.fieldOfView = Mathf.Lerp(cams.fieldOfView, RunFOV, CamsLerpTime * Time.deltaTime);
        } else if (Input.GetKeyUp(KeyCode.LeftShift)){ // đi
            CurrSpeed = WalkSpeed;
            // tắt hiệu ứng camera khi đi
            cams.fieldOfView = Mathf.Lerp(cams.fieldOfView, CamsNormalFov , CamsLerpTime * Time.deltaTime);
        }
    }
    void jumping(){
        if (Input.GetKeyDown(KeyCode.Space) && jumpleft > 0 && jumpable){
            // đặt lại trọng lực khi nhảy
            rbc.velocity = new Vector3(rbc.velocity.x, 0, rbc.velocity.z);
            rbc.AddForce(transform.up * jumpforce, ForceMode.VelocityChange); // bắt đầu nhảy
            jumpable = false;
            jumpleft--;
            Invoke(nameof(ReJump), 0.3f); // hạn chế thời gian nhảy để tránh nhảy liên tục
        }
    }
    void ReJump(){
        jumpable = true;
    }
    void OnCollisionEnter(Collision other) { // khi va chạm với vật
        if (other.collider.CompareTag("Ground")) {
            jumpleft = jumptime; // đặt lại số lần nhảy khi chạm đất
        }
    }
}