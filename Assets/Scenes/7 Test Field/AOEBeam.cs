using UnityEngine;
public class AOEBeam : MonoBehaviour {
    // sử dụng cho AOE và Laser
    [SerializeField] bool AOE;
    public float Damage;
    public void SetDmg(float NewDmg){
        Damage = NewDmg;
        if (!AOE) Damage *= Time.deltaTime;
    }
    void OnTriggerEnter(Collider other){ // sát thương tức thời
        if (other.CompareTag("Player") && AOE) other.GetComponent<PlayerHealthSystem>().HealthRestore(-Damage, false);
    }
    void OnTriggerStay(Collider other) { // sát thương theo thời gian
        if (other.CompareTag("Player") && !AOE) other.GetComponent<PlayerHealthSystem>().HealthRestore(-Damage, false);
    }
}