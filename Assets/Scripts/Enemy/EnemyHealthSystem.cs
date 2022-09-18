using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class EnemyHealthSystem : MonoBehaviour {
    // nhận sát thương chung cho mục tiêu
    [SerializeField] GameObject DamagePopPrefab; // hiển thị số sát thương đã gây ra
    [SerializeField] ParticleSystem particleSystem; // hiệu ứng khi trúng đòn
    public bool Alive = true; // điều kiện sống hay chết
    [SerializeField] Transform DamageSpawnPos; // vị trí sát thương hiển thị
    Transform LookPos; // hướng nhìn, cần để hiện số sát thương
    public Image HealthBar; // thanh máu hiển thị
    SoundManager soundManager; // âm thanh
    float HealthReduceSpeed = 0.3f; // tốc độ máu giảm xuống, đề xuất 0.3f
    public float ECurrHP, EMaxHP; // số máu
    [HideInInspector] public bool Blocking, CanTakeDamage; // điều kiện chặn đòn, nhận sát thương
    void Awake(){
        LookPos = FindObjectOfType<BPCR_BasicPlayerControlRigid>().transform;
        soundManager = GetComponent<SoundManager>();
        HealthBar.fillAmount = 1f; // đặt hiển thị máu (chia cho máu tối đa để lấy tỉ lệ % của 1)
        CanTakeDamage = true;
        Alive = true;
    }
    public void HealthSetting(float MinSet, float MaxSet){ // đặt thông số máu
        // đặt máu ngẫu nhiên
        EMaxHP = Random.Range(MinSet, MaxSet);
        ECurrHP = EMaxHP;
    }
    void LateUpdate(){
        // EnemyControl();
        HPBarDisplay();
    }
// gây sát thương trực tiếp cho mục tiêu, chỉ để thử nghiệm
    void EnemyControl(){
        if (Input.GetKeyDown(KeyCode.Space)) {
            bool Criting = false;
            if (Random.Range(0, 100) < 50) Criting = true;
            TakeDamage(Random.Range(20, 40), Criting); 
        } // gây sát thương
    }
// hiện và ẩn thanh máu theo khoảng cách
    void HPBarDisplay(){ 
    // giấu thanh máu khi cách xa
        if(Vector3.Distance(transform.position, LookPos.position) > 10f){ 
            HealthBar.enabled = false;
        } else {
            HealthBar.enabled = true; // hiện khi ở gần
            HealthBar.transform.LookAt(Camera.main.transform); // định hướng thanh máu về phía người chơi
        }
    }
// hệ thống nhận sát thương
    public void TakeDamage(float DamageTake, bool Crits){ // nhận sát thương
        if (Alive && CanTakeDamage){
            float Damage = ((int)DamageTake);

            if (!Blocking) { // nhận sát thương
                // hiện sát thương gây ra
                GameObject Clones = Instantiate(DamagePopPrefab, DamageSpawnPos.position, transform.rotation);
                Clones.GetComponentInChildren<TextMesh>().text = Damage.ToString();
                Clones.transform.LookAt(LookPos, Vector3.up);
                // đổi màu với loại sát thương
                if (Crits) Clones.GetComponentInChildren<TextMesh>().color = new Color(1f, 0f, 0f);
                else Clones.GetComponentInChildren<TextMesh>().color = new Color(1f, 1f, 1f);
                
                ECurrHP -= Damage; // thay đổi máu
                if (ECurrHP <= 0){
                soundManager.Play("TakeDamage" + Random.Range(0,3));
                // đặt thông số thành 0
                ECurrHP = 0;
                HealthBar.fillAmount = 0;
                Alive = false;
            }
                // nhận sát thương
                soundManager.Play("TakeDamage" + Random.Range(0,3)); // tiếng nhận sát thương
                StartCoroutine(HealthChange(ECurrHP / EMaxHP)); // hiển thị máu giảm xuống theo thời gian
                particleSystem.Play(); // thêm hiệu ứng
            } else { // hồi máu khi đang chặn đòn
                // hiện sát thương gây ra
                GameObject Clones = Instantiate(DamagePopPrefab, transform.position, transform.rotation);
                Clones.GetComponentInChildren<TextMesh>().text = (Damage * 0.75f).ToString();
                Clones.GetComponentInChildren<TextMesh>().color = Color.green; // đổi màu chữ
                // nhận sát thương
                soundManager.Play("TakeDamage" + Random.Range(0,3)); // tiếng nhận sát thương
                ECurrHP += (Damage * 0.75f); // thay đổi máu
                StartCoroutine(HealthChange(ECurrHP / EMaxHP)); // hiển thị máu giảm xuống theo thời gian
            }
        }
    }
    public IEnumerator HealthChange(float healthPCT){
        float PreChange = HealthBar.fillAmount; // thông số của lần chạy code trước
        float Elaps = 0; // cần cho tính thời gian trôi qua
        while (Elaps < HealthReduceSpeed){
            Elaps += Time.deltaTime; // tăng số theo thời gian
            HealthBar.fillAmount = Mathf.Lerp(PreChange, healthPCT, Elaps / HealthReduceSpeed); // giảm máu theo thời gian
            yield return null; // kết thúc chuỗi lệnh
        }
        HealthBar.fillAmount = healthPCT; // đặt máu cho đúng
    }
    public void Revive(){ // hồi sinh với máu bị giảm đi chút
        ECurrHP = EMaxHP;
        CanTakeDamage = true;
        EMaxHP = Random.Range(ECurrHP, EMaxHP) / 1.5f;
        float HealthPCT = ECurrHP / EMaxHP; // cần cho máu giảm theo thời gian
        StartCoroutine(HealthChange(HealthPCT));
    }
}