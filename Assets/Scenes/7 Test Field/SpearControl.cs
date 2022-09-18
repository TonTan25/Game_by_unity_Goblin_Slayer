using System.Collections;
using UnityEngine;

public class SpearControl : MonoBehaviour {
    // chỉ áp dụng cho thương của Grim Drake
    Transform Target; // mục tiêu
    // thông số bay, độ xoay khi sạc, độ xoay khi bay, phạm vi nổ, thời gian nổ
    [SerializeField]float FlySpeed, ChargeRotSpeed, SpearLifeTime, explotionRad, ExplotionTime;
    [SerializeField]GameObject SpearObject, Sphere;
    public bool Charging; // điều kiện
    [SerializeField]float Dmg; // sát thương
    SoundManager sounds = null; // âm thanh
    [SerializeField] ParticleSystem SpearPartical;
    Coroutine SpearMoving;
    void Start(){
        SpearOn(10);
    }
    public void SpearOn(float NewDamage){ // sắp đặt thông số
        sounds = GetComponent<SoundManager>();
        Dmg = NewDamage; // đặt sát thương
        Sphere.SetActive(false);
        Target = FindObjectOfType<PlayerHealthSystem>().gameObject.transform; // tìm mục tiêu
        Charging = true; // bật chế độ sạc
//sounds.Play("SpearSounds"); // âm thanh
        gameObject.transform.localScale = Vector3.one * 0.01f; // đặt lại độ lớn
        SpearMoving = StartCoroutine(SpearSet()); // đặt thương
        var Partical = SpearPartical.emission;
        Partical.enabled = false;
    }
    IEnumerator SpearSet(){
        GetComponent<Collider>().enabled = false;
        // tạo
        while (gameObject.transform.localScale.x <= 1f){
            gameObject.transform.localScale += Vector3.one * 0.05f;
            transform.LookAt(Target.position); // hướng về khi đang sạc
            yield return null;
        }
        gameObject.transform.localScale = new Vector3(1,1,1);
        var Partical = SpearPartical.emission;
        Partical.enabled = true;
        // sạc
        while(Charging){
            SpearObject.transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * ChargeRotSpeed);
            transform.LookAt(Target.position); // hướng về khi đang sạc
            yield return null;
        }
        GetComponent<Collider>().enabled = true;
        // bắn
        Vector3 MoveDir = (Target.position - transform.position).normalized;
        float Elaps = 0;
        while (Elaps < SpearLifeTime){
            SpearObject.transform.Rotate(new Vector3(0, 1, 0) * Time.deltaTime * ChargeRotSpeed * 5f); // xoáy nhanh
            transform.position += (MoveDir * FlySpeed * Time.deltaTime); // bay tới
            Elaps += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject, 10f);
    }
    public void SpearShot(){
        Charging = false;
    }
    void OnTriggerEnter(Collider other) { // khi va chạm
        if(other.CompareTag("Player")) SpearDamage(other.gameObject); // nếu trúng người chơi
        StartCoroutine(Explotion());
        // sounds.Play("SpearImpact"); // âm thanh va chạm
    }
    IEnumerator Explotion(){ // nổ
        StopCoroutine(SpearMoving);
        SpearObject.SetActive(false); // tắt hình thương
        var Partical = SpearPartical.emission;
        Partical.enabled = false;
        Sphere.SetActive(true);
        Vector3 Expl0 = Vector3.zero;
        Vector3 ExplTo = Vector3.one * Random.Range(explotionRad * 0.9f, explotionRad * 1.1f);
        Vector3 ExplCur = Expl0;
        Sphere.transform.localScale = Expl0;
        float Elaps = 0f;
        // nổ
        while (Elaps < ExplotionTime){
            Elaps += Time.deltaTime;
            Sphere.transform.localScale = Vector3.Lerp(ExplCur, ExplTo, Elaps / ExplotionTime);
            yield return null;
        }
        Sphere.transform.localScale = ExplTo;
        yield return new WaitForSeconds(0.5f);
        ExplCur = Sphere.transform.localScale;
        Elaps = 0f;
        while (Elaps < ExplotionTime * 1.5f){
            Sphere.transform.localScale = Vector3.Lerp(ExplCur, Expl0, Elaps / (ExplotionTime * 1.5f));
            Elaps += Time.deltaTime;
            yield return null;
        }
        Sphere.transform.localScale = Expl0;
        Destroy(gameObject, 5f);
    }
    void SpearDamage(GameObject Player){ // khi chạm sẽ phát nổ
        GetComponent<Collider>().enabled = false;
        Player.GetComponent<PlayerHealthSystem>().TakeDamage(Dmg); // lấy component và nhả sát thương
    }
}