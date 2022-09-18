using UnityEngine.AI; // cần để có thể điều khiển AI
using UnityEngine;
public class EnemySlimeControl : MonoBehaviour {
    // chỉ áp dụng cho Slime
    // chỉ có trạng thái đuổi theo và choáng
    // không có trạng thái đứng chờ hoặc lang thang
    // dùng enum để chỉ định trạng thái hiện tại của AI
    // khá dễ để né đòn đánh
    enum State { StandFirm, Moving }
    public EntityUseStats EStats; // cần để lấy tốc độ di chuyển
    public LayerMask PlayerLayer; // cần cho việc thực hiện di chuyển, gây sát thương cho người chơi
    public float AttackRange; // phạm vi tấn công
    [SerializeField] float SizeMinScale;
    [SerializeField] float SizeMaxScale;
    public bool DrawlAttackRange, PinkSlime; // vẽ tầm tấn công, ects
    float BasicSpeed, Distant; // tốc độ cơ bản, khoảng cách
    bool attacked; // điều kiện đã tấn công
    State state; // lấy trạng thái của AI
    SoundManager soundManager; // âm thanh cho chạy, nhảy, nhận sát thương, v.v..+
    PlayerHealthSystem playerHealthSystem; // xác nhận đã chết hay chưa
    EnemyHealthSystem EhealthSystem; // trạng thái đã chết hay chưa
    DropItems dropItems; // tính năng rơi đồ
    Animator animator; // thực hiện hành động và động tác
    NavMeshAgent agent; // AI đi lại
    Transform Target; // điểm AI sẽ nhìn vào khi chạy đến và tấn công
    void Start(){
        dropItems = GetComponent<DropItems>(); // lấy rơi đồ
        EhealthSystem = GetComponent<EnemyHealthSystem>(); // lấy thông tin còn sống
        playerHealthSystem = FindObjectOfType<PlayerHealthSystem>(); // lấy mục máu của người chơi
        soundManager = GetComponent<SoundManager>(); // lấy âm thanh
        animator = GetComponent<Animator>(); // lấy hoạt ảnh
        agent = GetComponent<NavMeshAgent>(); // lấy phần AI

        gameObject.transform.localScale = Vector3.one * Random.Range(SizeMinScale, SizeMaxScale); // định dạng to nhỏ ngẫu nhiên
        BasicSpeed = Random.Range(EStats.UseSpeed * 0.8f, EStats.UseSpeed * 1.2f); // đặt tốc dộ bình thường
        agent.speed = BasicSpeed; // đặt tốc độ cơ bản

        attacked = false;
        Target = GameObject.FindGameObjectWithTag("Player").transform; // lấy điểm sẽ nhìn vào
        soundManager.Play("Spawning"); // chơi âm thanh khi sinh ra
        state = State.StandFirm; // đặt trạng thái khi xuất hiện
        EhealthSystem.HealthSetting(EStats.UseHealthMin, EStats.UseHealthMax); // đặt máu
    }
    void LateUpdate() {
        // đổi trạng thái tùy theo tình huống
        switch (state) {
            default:
            case State.StandFirm: // trạng thái cơ bản, không làm gì hết

            break;

            case State.Moving:// đuổi theo
                StartChase();
            break;
        }
        // xem tình trạng còn sống hay không
        if (!EhealthSystem.Alive){
            Ded();
        }
    }
    void StartChase(){
        // lấy cự ly                        điểm đầu                điểm cuối
        Distant = Vector3.Distance(transform.position, Target.position) - 1f;
        if (Distant > 1f){ // chạy tới
            animator.SetFloat("Walk", 1); // chơi hoạt ảnh
            agent.SetDestination(Target.position); // đặt điểm đến và chạy tới
        } else if (Distant <= AttackRange) { // tấn công
            Stational();
            Attacking();
        }
    }
    void Attacking(){
        if (playerHealthSystem.Alive && !attacked){
            transform.LookAt(Target.position); // đổi hướng rồi tấn công
            animator.SetTrigger("Attack"); // tấn công
            attacked = true; // xác nhận đã tấn công
        }
    }
    void Stational(){ // cố định vị trí
        agent.SetDestination(transform.position);
        state = State.StandFirm;
    }
    void Ded(){ // tạch !
        EhealthSystem.CanTakeDamage = false;
        EhealthSystem.Alive = true;
        Stational();
        animator.enabled = false; // đóng băng hoạt ảnh
        gameObject.AddComponent<Rigidbody>(); // thêm trọng lực
        dropItems.DropSomething(EStats.EntityIndext); // rơi vật phẩm
        agent.enabled = false; // tắt di chuyển
        FindObjectOfType<WaveControl>().EntityAmountReduce();
        FindObjectOfType<AchiveIGTrack>().AchivePorgress(2, true, 1);
        FindObjectOfType<AchiveIGTrack>().AchivePorgress(3, true, 1);
        FindObjectOfType<AchiveIGTrack>().AchivePorgress(4, true, 1);
        FindObjectOfType<AchiveIGTrack>().AchivePorgress(5, true, 1);
        FindObjectOfType<AchiveIGTrack>().AchivePorgress(6, true, 1);
        if (PinkSlime) {
            FindObjectOfType<AchiveIGTrack>().AchivePorgress(7, true, 1);
            FindObjectOfType<AchiveIGTrack>().AchivePorgress(8, true, 1);
        }
        Destroy(gameObject, 10f); // xóa vật sau thời gian nhất định
        this.enabled = false;
    }
    void EVLookat(){
        transform.LookAt(Target.transform);
    }
    void EventAttack(){ // đòn tấn công
        // thu thập thông tin từ đòn đánh với :     đểm đánh                tầm đánh       lớp lấy thông tin (layermask)
        Collider[] HitPlayer = Physics.OverlapSphere(transform.position, AttackRange * 3f, PlayerLayer);
        
        foreach(Collider Player in HitPlayer){ // gây sát thương cho người chơi nếu trúng
            float DamageDone = Random.Range(EStats.UseMinDmg, EStats.UseMaxDmg + 1); // gây 1 lượng sát thương ngẫu nhiên
            Player.GetComponent<PlayerHealthSystem>().TakeDamage(DamageDone); // lấy component và nhả sát thương
        }
    }
    void EVAfterAtkCheck(){ // sau khi tung đòn đánh sẽ kiểm tra vị trí của 2 bên
        attacked = false;
        Distant = Vector3.Distance(transform.position, Target.position);
        if (Distant < AttackRange){
            Attacking();
        } else state = State.Moving;
    }
    void EventFootSteps(){ // chơi tiếng động khi bước đi
        soundManager.Play("Step" + Random.Range(0,2));
    }
    void OnDrawGizmos() {
        if (DrawlAttackRange){
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, AttackRange); // hiện tầm
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange); // hiện tầm đánh
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, AttackRange * 3f); // hiện tầm gây sát thương
        }
    }
}