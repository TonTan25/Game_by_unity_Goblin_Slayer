using UnityEngine;

public class WeaponsSway : MonoBehaviour
{
    // di chuyển vũ khí cho mượt mà hơn
    // gắn vào parent vũ khí
    public float SmoothTime, SmoothForce;
    void Update()
    {
        // lấy số đầu vào
        Vector2 RotateInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y") * SmoothForce); 
        // tính toán các thứ
        Quaternion RotationX = Quaternion.AngleAxis(-RotateInput.x, Vector3.right);
        Quaternion RotationY = Quaternion.AngleAxis(RotateInput.y, Vector3.up);
        Quaternion RotateTarget = RotationX * RotationY;
        // xoay
        transform.localRotation = Quaternion.Slerp(transform.localRotation, RotateTarget, SmoothTime * Time.deltaTime);
    }
}