using UnityEngine;
public class DamagePopScript : MonoBehaviour
{
    // hiện số sát thương khi dùng
    public float RanVal;
    public void Start(){
        // đặt vị trí ngẫu nhiên khi xuất hiện
        Vector3 Pos = new Vector3(Random.Range(-RanVal, RanVal), Random.Range(-RanVal, RanVal), Random.Range(-RanVal, RanVal));
        transform.position += Pos;
        Destroy(gameObject, 1.2f);
    }
}