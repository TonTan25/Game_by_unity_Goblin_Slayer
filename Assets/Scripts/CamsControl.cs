using UnityEngine;

public class CamsControl : MonoBehaviour {
    // sử dụng để xoay camera
    public float MouseSensitiv; // tốc độ chuột, đề xuất 2
    public Transform CamsHolder; // lấy camera
    float XRotate, YRotate; // góc độ xoay
    Vector2 RotateInput; // đầu vào khi di chuột
    void Update() {
        CamsInput();
    }
    public void CamsInput(){
        // nhập số tương ứng khi di chuột
        RotateInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")); // lấy số đầu vào
        YRotate += RotateInput.x * MouseSensitiv; // xoay góc nhìn qua trái, phải tương ứng
        XRotate -= RotateInput.y * MouseSensitiv; // xoay góc nhìn lên xuống tương ứng với chuột
        XRotate = Mathf.Clamp(XRotate, -80f, 85f); // giới hạn góc nhìn lên xuống để khỏi gãy cổ
        CamsHolder.transform.localRotation = Quaternion.Euler(XRotate, YRotate, 0f);// bắt đầu xoay góc nhìn
        transform.rotation = Quaternion.Euler(0, YRotate, 0); // bắt đầu xoay góc nhìn
    }
}