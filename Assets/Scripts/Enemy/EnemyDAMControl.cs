using UnityEngine.AI; // cần để có thể điều khiển AI
using UnityEngine;
public class EnemyDAMControl : MonoBehaviour
{
    // chỉ áp dụng cho hắc tiên dùng kiếm
    // không có trạng thái đứng chờ hoặc lang thang
    // dùng enum để chỉ định trạng thái hiện tại của AI
    // khá dễ để né đòn đánh
    enum State { StandFirm, Moving, Chasing }
    State state; // lấy trạng thái của AI
    public EntityUseStats EStats; // cần để lấy tốc độ di chuyển
    public LayerMask PlayerLayer; // cần cho việc thực hiện di chuyển, gây sát thương cho người chơi
    public Transform attackpointset; // điểm tấn công và phạm vi tấn công (đặt tầm đánh nhỏ hơn tầm gây sát thương)
    public float AttackRange, SpecialCooldown; // phạm vi tấn công / thời gian hồi của đòn đặc biệt
    [SerializeField][Range(0.5f, 1f)] float SizeMinScale;
    [SerializeField][Range(1f, 2f)] float SizeMaxScale;
    public bool DrawlAttackRange; // vẽ tầm tấn công, ects
    bool attacked, CanUseSpecial; // đã tấn công hay chưa / có thể dùng đặc biệt
    float BasicSpeed, SpecialSpeed, Distant; // tốc độ cơ bản / tốc độ khi đang dùng đòn đặc biệt
    SoundManager soundManager; // âm thanh cho chạy, nhảy, nhận sát thương, v.v..+
    PlayerHealthSystem playerHealthSystem; // xác nhận đã chết hay chưa
    EnemyHealthSystem EhealthSystem; // cần cho trạng thái đã chết hay chưa
    DropItems dropItems; // tính năng rơi đồ
    Animator animator; // thực hiện hành động và động tác
    NavMeshAgent agent; // cần cho AI đi lại
    Transform Target; // điểm AI sẽ nhìn vào khi chạy đến và tấn công
    TrailRenderer trails;
    Rigidbody RB;
    void Start(){
        RB = GetComponent<Rigidbody>();
        trails = GetComponentInChildren<TrailRenderer>();
        dropItems = GetComponent<DropItems>(); // lấy rơi đồ
        EhealthSystem = GetComponent<EnemyHealthSystem>(); // lấy thông tin còn sống
        playerHealthSystem = FindObjectOfType<PlayerHealthSystem>(); // lấy mục máu của người chơi
        soundManager = GetComponent<SoundManager>(); // lấy âm thanh
        animator = GetComponent<Animator>(); // lấy hoạt ảnh
        agent = GetComponent<NavMeshAgent>(); // lấy phần AI
        
        gameObject.transform.localScale = Vector3.one * Random.Range(SizeMinScale, SizeMaxScale); // định dạng to nhỏ ngẫu nhiên
        BasicSpeed = Random.Range(EStats.UseSpeed * 0.8f, EStats.UseSpeed * 1.2f); // đặt tốc dộ bình thường
        SpecialSpeed = BasicSpeed * Random.Range(1.1f, 1.5f); // đặt tốc độ khi chạy
        agent.speed = BasicSpeed;
        CanUseSpecial = true;

        Target = GameObject.FindGameObjectWithTag("Player").transform; // lấy điểm sẽ nhìn vào
        soundManager.Play("Spawning"); // chơi âm thanh khi sinh ra
        state = State.StandFirm; // đặt trạng thái khi xuất hiện
        EhealthSystem.HealthSetting(EStats.UseHealthMin, EStats.UseHealthMax); // đặt máu
    }
    void LateUpdate() {
        // đổi trạng thái tùy theo tình huống
        switch (state) {
            default:
            case State.StandFirm: // đuổi theo
                // trạng thái trống
            break;

            case State.Moving: // khi dùng đặc biệt
                StartNormalChase();
            break;

            case State.Chasing:
                StartSpecialMode();
            break;
        }
        // xem tình trạng còn sống hay không
        if (!EhealthSystem.Alive) Ded();
    }
    void StartNormalChase(){
        // lấy cự ly                        điểm đầu                điểm cuối
        Distant = Vector3.Distance(transform.position, Target.position);
        if (Distant > 2.2f){ // chạy tới bình thường
            animator.SetFloat("Walk", 1); // chơi hoạt ảnh bước
            agent.SetDestination(Target.position); // đặt điểm đến và chạy tới
        } else { // tấn công
            animator.SetFloat("Walk", 0); // chơi hoạt ảnh bước
            Stational();
            if (playerHealthSystem.Alive && !attacked){
                animator.SetTrigger("Attack" + Random.Range(0, 2)); // tấn công
                attacked = true; // xác nhận đã tấn công
            }
        }
    }
    void StartSpecialMode(){ // trạng thái đặc biệt
        // lấy cự ly                        điểm đầu                điểm cuối
        Distant = Vector3.Distance(transform.position, Target.position);
        // bước tới bình thường
        if (Distant > 2.2f) { // chạy tới
            animator.SetFloat("Run", 1); // chơi hoạt ảnh bước
            agent.SetDestination(Target.position); // đặt điểm đến và chạy tới
        } else { // tấn công
            animator.SetFloat("Run", 0); // dừng hoạt ảnh
            Stational();
            if (playerHealthSystem.Alive){
                if (!attacked && CanUseSpecial) { // đỡ đòn đánh
                    animator.speed = 1f;
                    animator.SetTrigger("Block");
                    CanUseSpecial = false;
                    attacked = true;
                } else if (!attacked && !CanUseSpecial) {
                    animator.speed = 1.5f; // tăng tốc độ đánh
                    animator.SetTrigger("Attack" + Random.Range(0, 2)); // tấn công
                    attacked = true; // xác nhận đã tấn công
                } 
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
    public void EventAttack(){ // đòn tấn công
        // thu thập thông tin từ đòn đánh với :     đểm đánh                tầm đánh       lớp lấy thông tin (layermask)
        Collider[] HitPlayer = Physics.OverlapSphere(attackpointset.position, AttackRange * 1.5f, PlayerLayer);
        
        foreach(Collider Player in HitPlayer){ // gây sát thương cho người chơi nếu trúng
            float DamageDone = Random.Range(EStats.UseMinDmg, EStats.UseMaxDmg + 1); // gây 1 lượng sát thương ngẫu nhiên
            Player.GetComponent<PlayerHealthSystem>().TakeDamage(DamageDone); // lấy component và nhả sát thương
        }
    }
    void EventEnableTrail(){
        if(trails) trails.emitting = true;
    }
    void EventDisableTrail(){
        if(trails) trails.emitting = false;
    }
    void EventAfterAttack(){ // sau khi tung đòn đánh sẽ kiểm tra vị trí của 2 bên
        attacked = false;
        animator.speed = 1f;
        EVHealthCheck();
    }
    void EventPlayAttackSound(){ // chơi tiếng động khi tấn công
        soundManager.Play("Swing" + Random.Range(0,3)); 
    }
    void EventFootSteps(){ // chơi tiếng động khi bước đi
        soundManager.Play("Step" + Random.Range(0,3));
    }
    void EventStartBlocking(){ // bắt đầu đỡ đòn
        EhealthSystem.Blocking = true;
    }
    void EventSpecialReset(){ // đặt lại đòn đánh đặc biệt với số thời gian đã cho
        EhealthSystem.Blocking = false;
        animator.speed = 1f;
        Invoke(nameof(SpecialReset), Random.Range(SpecialCooldown * 0.8f, SpecialCooldown));
    }void EVLookat(){
        transform.LookAt(Target.transform);
    }
    void SpecialReset(){ // đặt lại đòn đánh đặc biệt
        attacked = false;
        CanUseSpecial = true;
    }
    void EVHealthCheck(){
        // kiểm tra lượng máu đổi trạng thái
        if (EhealthSystem.ECurrHP <= EhealthSystem.EMaxHP / 2){
            agent.speed = SpecialSpeed; // tăng tốc độ
            state = State.Chasing;
        } else {
            agent.speed = BasicSpeed;
            state = State.Moving;
        }
    }
    void OnDrawGizmos() {
        if (DrawlAttackRange){
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, AttackRange); // hiện tầm
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackpointset.position, AttackRange); // hiện tầm đánh
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackpointset.position, AttackRange * 1.5f); // hiện tầm gây sát thương
        }
    }
}