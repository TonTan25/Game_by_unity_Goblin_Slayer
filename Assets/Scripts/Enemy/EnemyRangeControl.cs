using UnityEngine.AI; // cần để có thể điều khiển AI
using UnityEngine;
public class EnemyRangeControl : MonoBehaviour
{
    // chỉ áp dụng cho lính đánh xa với 1 hành động
    // chỉ có trạng thái đuổi theo và choáng
    // không có trạng thái đứng chờ hoặc lang thang
    // dùng enum để chỉ định trạng thái hiện tại của AI
    // khá dễ để né đòn đánh
    enum State { StandFirm, Moving }
    public EntityUseStats EStats; // cần để lấy tốc độ di chuyển
    public LayerMask PlayerLayer; // cần cho việc thực hiện di chuyển, gây sát thương cho người chơi
    public Transform AttackSet; // điểm đầu phạm vi bắn
    public float AttackRange; // phạm vi tấn công
    public bool DrawlAttackRange; // vẽ tầm tấn công, ects
    [SerializeField][Range(0.5f, 1f)] float SizeMinScale;
    [SerializeField][Range(1f, 2f)] float SizeMaxScale;
    public GameObject Bullet; // vật sẽ bắn ra
    float BasicSpeed, Distant; // tốc độ cơ bản, khoảng cách giữa mục tiêu
    bool attacked; // đã tấn công hay chưa
    State state; // lấy trạng thái của AI
    SoundManager soundManager; // âm thanh cho chạy, nhảy, nhận sát thương, v.v..+
    PlayerHealthSystem playerHealthSystem; // xác nhận đã chết hay chưa
    EnemyHealthSystem EhealthSystem; // cần cho trạng thái đã chết hay chưa
    DropItems dropItems; // tính năng rơi đồ
    Animator animator; // thực hiện hành động và động tác
    NavMeshAgent agent; // cần cho AI đi lại
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
        agent.speed = BasicSpeed; // đặt tốc độ mặc định

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
        if (!EhealthSystem.Alive) Ded();
    }
    void StartChase(){
        // lấy cự ly                        điểm đầu                điểm cuối
        Distant = Vector3.Distance(transform.position, Target.position) - 1f;
        if (Distant > AttackRange * 0.8f){ // chạy tới
            animator.SetFloat("Walk", 1); // chơi hoạt ảnh
            agent.SetDestination(Target.position); // đặt điểm đến và chạy tới
        } else if (Distant <= AttackRange) { // tấn công
            animator.SetFloat("Walk", 0); // tắt hoạt ảnh
            Stational();
            if (playerHealthSystem.Alive && !attacked){
                animator.SetTrigger("Attack"); // tấn công
                attacked = true; // xác nhận đã tấn công
            }
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
        Destroy(gameObject, 10f); // xóa vật sau thời gian nhất định
        this.enabled = false;
    }
    void EventAttack(){ // đòn tấn công
        GameObject Clone = Instantiate(Bullet, AttackSet.position, Quaternion.identity); // tạo 1 mũi tên
        float DamageDone = Random.Range(EStats.UseMinDmg, EStats.UseMaxDmg + 1); // gây 1 lượng sát thương ngẫu nhiên
        Clone.GetComponent<RangeBullets>().Shoot(Target.position + transform.up / 2, DamageDone);
        soundManager.Play("Swing" + Random.Range(0,3));
        // sử dụng script tại mũi tên để tạo sát thương
    }
    void EVLookat(){
        transform.LookAt(Target.transform);
    }
    void EventAfterAttack(){ // sau khi tung đòn đánh sẽ kiểm tra vị trí của 2 bên
        attacked = false;
        state = State.Moving;
    }
    void EventFootSteps(){ // chơi tiếng động khi bước đi
        soundManager.Play("Step" + Random.Range(0,2));
    }
    void OnDrawGizmos(){
        if (DrawlAttackRange){
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(AttackSet.position, AttackRange); // hiện tầm
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange); // hiện tầm đánh
        }
    }
}