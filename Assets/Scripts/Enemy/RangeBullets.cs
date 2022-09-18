using System.Collections; // cần cho mũi tên bay tới
using UnityEngine;

public class RangeBullets : MonoBehaviour
{
    // chỉ áp dụng cho đạn
    public float TravelTime, RotateForce, TravelForce; // lực bắn
    float DamageTake; // lưu lại sát thương
    public void Shoot(Vector3 Pos, float Damage){
        StartCoroutine(LlerpTo(Pos));
        DamageTake = Damage;
    }
    public IEnumerator LlerpTo(Vector3 Pos){
        Vector3 ToPos = (Pos - transform.position).normalized; // lấy vị trí từ lần di chuyển trước
        transform.LookAt(Pos); // hướng về phía mục tiêu
        float elaps = 0f; // tính thời gian
        while (elaps < TravelTime){
            elaps += Time.deltaTime; // tăng lên theo thời gian
            transform.position += (ToPos * TravelForce * Time.deltaTime); // bay tới nơi được chỉ định
            transform.Rotate(new Vector3(1,0,0) * RotateForce * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject); // xóa vật
    }
    void OnTriggerEnter(Collider other) {
        if(other.tag == "Player"){
            other.gameObject.GetComponent<PlayerHealthSystem>().TakeDamage(DamageTake);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            StopAllCoroutines();
            Destroy(gameObject, 2f);
        }
    }
}