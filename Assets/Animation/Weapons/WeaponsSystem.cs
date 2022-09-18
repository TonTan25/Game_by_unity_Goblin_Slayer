using UnityEngine;
public class WeaponsSystem : MonoBehaviour
{
    // gắn vào xương hoạt ảnh, chỉ dùng để kích hoạt hoạt ảnh
    public Transform AttackSet; // điểm tấn công
    public LayerMask EnemyLayer;
    [SerializeField] float AttackRange, CurCritDamage;
    [SerializeField] int CurMindamage, CurMaxdamage, CurCritchance;
    [SerializeField][Range(1,4f)] float SkillRange, DmgScale4S, DmgScale1S, DmgScalePoke; // tỉ lệ sát thương
    [SerializeField] [Range(0,0.5f)] float HealScale1S;
    public bool DrawlAttackRange;
    [SerializeField]WeaponInfo WPinfo;
    [SerializeField]PlayerCombatSystem combatSystem;
    [SerializeField] PlayerHealthSystem healthSystem;
    SoundManager sounds;
    public Animator WPanimate;
    float MovementInput;
    void Start(){
        WPanimate = GetComponent<Animator>();
        sounds = GetComponent<SoundManager>();
        healthSystem = FindObjectOfType<PlayerHealthSystem>();
    }
    void LateUpdate(){
        MovementInput = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
        WPanimate.SetFloat("Moving", MovementInput);
    }
    public void DoNormalAttack(int A){ // đánh thường
        if (A < 2) WPanimate.SetTrigger("Attack" + Random.Range(0, 3));
        else WPanimate.SetTrigger("Attack" + Random.Range(0, 6));
    }
    public void DoPokeAttack(){
        WPanimate.SetTrigger("Poke");
    }
    public void DoSkillAttack(int SkillNumb){
        WPanimate.SetTrigger("Skill" + SkillNumb);
    }
    public void SwitchWPStat(int WPNumber){
        // đặt thông số
        CurMindamage = WPinfo.weapons[WPNumber].MinDamage; // sát thương tối thiểu
        CurMaxdamage = WPinfo.weapons[WPNumber].MaxDamage; // sát thương tối đa
        CurCritchance = WPinfo.weapons[WPNumber].CritChance; // tỉ lệ chí mạng
        CurCritDamage = WPinfo.weapons[WPNumber].CritDamage; // sát thương chí mạng
        WPanimate.speed = WPinfo.weapons[WPNumber].AtkSpeed; // tốc độ tấn công
    }
    void EventDoNormalDamage(){ // đánh thường
        // thu thập thông tin từ đòn đánh với :     đểm đánh            tầm đánh     lớp lấy thông tin (layermask)
        Collider[] Hit = Physics.OverlapSphere(AttackSet.position, AttackRange, EnemyLayer);
        for (int i = 0; i < Hit.Length; i++){
            float RawDamageDone = Random.Range(CurMindamage, CurMaxdamage + 1); // gây 1 lượng sát thương ngẫu nhiên (dùng float)
            bool Criting = false;
            if (Random.Range(0, 100) < CurCritchance){ // điều kiện chí mạng
                RawDamageDone *= CurCritDamage;
                Criting = true;
            }
            int DamageDone = ((int)RawDamageDone); // chuyển đổi lại thành số int
            Hit[i].gameObject.GetComponent<EnemyHealthSystem>().TakeDamage(DamageDone, Criting); // lấy component và nhả sát thương
        }
    }
    void EventDoPokeDamage(){ // đâm kiếm
        // thu thập thông tin từ đòn đánh với :     đểm đánh            tầm đánh     lớp lấy thông tin (layermask)
        Collider[] Hit = Physics.OverlapSphere(AttackSet.position, AttackRange * SkillRange, EnemyLayer);
        for (int i = 0; i < Hit.Length; i++){
            float RawDamageDone = Random.Range(CurMindamage, CurMaxdamage + 1) * DmgScalePoke; // gây 1 lượng sát thương ngẫu nhiên (dùng float)
            bool Criting = false;
            if (Random.Range(0, 100) < CurCritchance){ // xem có thể chí mạng hay không
                RawDamageDone *= CurCritDamage; // chí mạng thì sẽ tăng damage
                Criting = true;
            }
            int DamageDone = ((int)RawDamageDone); // chuyển đổi lại thành số int
            Hit[i].gameObject.GetComponent<EnemyHealthSystem>().TakeDamage(DamageDone, Criting); // lấy component và nhả sát thương
        }
    }
    void EventDoPowerSlashDamage(){ // chém mạnh 1 lần
        // thu thập thông tin từ đòn đánh với :     đểm đánh            tầm đánh     lớp lấy thông tin (layermask)
        Collider[] Hit = Physics.OverlapSphere(AttackSet.position, AttackRange * (SkillRange * 1.2f), EnemyLayer);
        float HealthHeal = 0; // tổng số máu sẽ hồi
        for (int i = 0; i < Hit.Length; i++){
            float RawDamageDone = Random.Range(CurMindamage, CurMaxdamage + 1) * DmgScale1S; // gây 1 lượng sát thương ngẫu nhiên (dùng float)
            bool Crit = false;
            if (Random.Range(0, 100) < CurCritchance){// điều kiện chí mạng
                RawDamageDone *= CurCritDamage; // chí mạng thì sẽ tăng damage
                Crit = true;
            }
            int DamageDone = ((int)RawDamageDone); // chuyển đổi lại thành số int
            Hit[i].gameObject.GetComponent<EnemyHealthSystem>().TakeDamage(DamageDone, Crit); // lấy component và nhả sát thương
            HealthHeal += RawDamageDone * HealScale1S; // tăng thêm số máu hồi với mỗi mục tiêu trúng
        }
        healthSystem.HealthRestore(HealthHeal, false);
    }
    void EventDoFourSlashDamage(){ // tấn công 4 lần
        // thu thập thông tin từ đòn đánh với :     đểm đánh            tầm đánh     lớp lấy thông tin (layermask)
        Collider[] Hit = Physics.OverlapSphere(AttackSet.position, AttackRange * SkillRange, EnemyLayer);
        for (int i = 0; i < Hit.Length; i++){
            float RawDamageDone = Random.Range(CurMindamage, CurMaxdamage + 1) * DmgScale4S; // gây 1 lượng sát thương ngẫu nhiên (dùng float)
            bool Crit = false;
            if (Random.Range(0, 100) < CurCritchance){ // điều kiện chí mạng
                RawDamageDone *= CurCritDamage;
                Crit = true;
            }
            int DamageDone = ((int)RawDamageDone); // chuyển đổi lại thành số int
            Hit[i].gameObject.GetComponent<EnemyHealthSystem>().TakeDamage(DamageDone, Crit); // lấy component và nhả sát thương
        }
        if (Hit.Length > 0) healthSystem.HealthRestore(Random.Range(6, 12), false); // hồi máu với mỗi đòn tung ra
    }
    public void EventEnableTrail(){ // bật hiệu ứng chém
        combatSystem.trails.emitting = true;
    }
    public void EventDisableTrail(){ // tắt hiệu ứng chém
        combatSystem.trails.emitting = false;
    }
    void EventRefreshAction(){ // làm mới hành động
        combatSystem.ActionAllowed = true;
    }
    void EventAttackSound(){ // tiếng tấn công
        sounds.Play("AttackSound" + Random.Range(0, 3));
    }
    void EventTakeOut(){ // tiếng thay trang bị
        sounds.Play("TakeOut");
    }
    void EventFootSteps(){ // tiếng bước chân
        sounds.Play("FootStep" + Random.Range(0, 2));
    }
    void OnDrawGizmos() { // tầm đánh
        if (DrawlAttackRange){
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(AttackSet.position, AttackRange); // hiện tầm
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(AttackSet.position, AttackRange * SkillRange); // hiện tầm
        }
    }
}