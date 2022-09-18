using UnityEngine.AI; // cần để có thể điều khiển AI
using UnityEngine;
public class EnemyHauntedControl : MonoBehaviour
{
    // chỉ áp dụng cho Haunted WoodStand
    // không có trạng thái đứng chờ hoặc lang thang
    // dùng enum để chỉ định trạng thái hiện tại của AI
    // khá dễ để né đòn đánh
    enum State { StandFirm, Moving, Special }
    State state; // lấy trạng thái của AI
    public EntityUseStats EStats; // cần để lấy tốc độ di chuyển
    public LayerMask PlayerLayer; // cần cho việc thực hiện di chuyển, gây sát thương cho người chơi
    public Transform attackpointset; // điểm tấn công và phạm vi tấn công (đặt tầm đánh nhỏ hơn tầm gây sát thương)
    public float AttackRange, SpecialCooldown; // phạm vi tấn công / thời gian hồi của đòn đặc biệt
    [SerializeField][Range(0.5f, 1f)] float SizeMinScale;
    [SerializeField][Range(1f, 2f)] float SizeMaxScale;
    public bool DrawlAttackRange; // vẽ tầm tấn công, ects
    bool attacked, CanSpecial; // đã tấn công hay chưa / có thể dùng đặc biệt hay không
    float BasicSpeed, SpecialSpeed, Distant; // tốc độ cơ bản / tốc độ đặc biệt
    SoundManager soundManager; // âm thanh cho chạy, nhảy, nhận sát thương, v.v..+
    PlayerHealthSystem playerHealthSystem; // xác nhận đã chết hay chưa
    EnemyHealthSystem EhealthSystem; // cần cho trạng thái đã chết hay chưa
    DropItems dropItems; // tính năng rơi đồ
    Animator animator; // thực hiện hành động và động tác
    NavMeshAgent agent; // cần cho AI đi lại
    Transform Target; // điểm AI sẽ nhìn vào khi chạy đến và tấn công
    TrailRenderer trails;
    void Start(){
        trails = GetComponentInChildren<TrailRenderer>();
        dropItems = GetComponent<DropItems>(); // lấy rơi đồ
        EhealthSystem = GetComponent<EnemyHealthSystem>(); // lấy thông tin còn sống
        playerHealthSystem = FindObjectOfType<PlayerHealthSystem>(); // lấy mục máu của người chơi
        soundManager = GetComponent<SoundManager>(); // lấy âm thanh
        animator = GetComponent<Animator>(); // lấy hoạt ảnh
        agent = GetComponent<NavMeshAgent>(); // lấy phần AI

        gameObject.transform.localScale = Vector3.one * Random.Range(SizeMinScale, SizeMaxScale); // định dạng to nhỏ ngẫu nhiên
        BasicSpeed = Random.Range(EStats.UseSpeed * 0.8f, EStats.UseSpeed * 1.2f); // đặt tốc dộ bình thường
        SpecialSpeed = BasicSpeed * Random.Range(2f, 3f); // đặt tốc độ khi chạy
        
        Target = GameObject.FindGameObjectWithTag("Player").transform; // lấy điểm sẽ nhìn vào
        soundManager.Play("Spawning"); // chơi âm thanh khi sinh ra
        EhealthSystem.HealthSetting(EStats.UseHealthMin, EStats.UseHealthMax); // đặt máu
        state = State.StandFirm; // đặt trạng thái khi xuất hiện
    }
    void LateUpdate() {
        // đổi trạng thái tùy theo tình huống
        switch (state) {
            default:
            case State.StandFirm: // đuổi theo
                // trạng thái trống
            break;

            case State.Moving: // khi dùng đặc biệt
                StartChase();
            break;

            case State.Special:
                StartSpecial();
            break;
        }
        // xem tình trạng còn sống hay không
        if (!EhealthSystem.Alive) Ded();
    }
    void StartChase(){
        // lấy cự ly                        điểm đầu                điểm cuối
        Distant = Vector3.Distance(transform.position, Target.position);
        // nếu ở quá xa sẽ đổi sang đòn đặc biệt
        if (Distant > 8f && CanSpecial){
            CanSpecial = false; // tắt điều kiện
            animator.SetFloat("Walk", 0); // dừng chơi hoạt ảnh
            state = State.Special; // đổi trạng thái
            agent.speed = SpecialSpeed; // đặt tốc độ
        } else if (Distant > 2f){ // chạy tới
            animator.SetFloat("Walk", 1); // chơi hoạt ảnh
            agent.SetDestination(Target.position); // đặt điểm đến và chạy tới
        } else { // tấn công
            state = State.StandFirm; // đứng im khi tấn công
            Stational();
            if (playerHealthSystem.Alive && !attacked){
                animator.SetTrigger("Attack" + Random.Range(0,2)); // tấn công
                attacked = true; // xác nhận đã tấn công
            }
        }
    }
    void StartSpecial(){ // trạng thái đặc biệt
        // lấy cự ly                        điểm đầu                điểm cuối
        Distant = Vector3.Distance(attackpointset.position, Target.position);
        if (Distant > 2f){ // chạy theo nếu không trong tầm đánh
            agent.SetDestination(Target.position); // đặt điểm đến và chạy tới
            animator.SetFloat("Run", 1); // chơi hoạt ảnh
        } else { // đứng im khi đã trong tầm
            animator.SetFloat("Run", 0); // dừng chơi hoạt ảnh
            Stational();
            if (playerHealthSystem.Alive && !attacked){ // tấn công
                animator.SetTrigger("SpecialAttack"); // chơi hoạt ảnh
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
    public void EventEnableTrail(){
        if(trails) trails.emitting = true;
    }
    public void EventDisableTrail(){
        if(trails) trails.emitting = false;
    }
    void EventAttack(){ // đòn tấn công
        // thu thập thông tin từ đòn đánh với :     đểm đánh                tầm đánh       lớp lấy thông tin (layermask)
        Collider[] HitPlayer = Physics.OverlapSphere(attackpointset.position, AttackRange * 1.5f, PlayerLayer);
        
        foreach(Collider Player in HitPlayer){ // gây sát thương cho người chơi nếu trúng
            float DamageDone = Random.Range(EStats.UseMinDmg, EStats.UseMaxDmg + 1); // gây 1 lượng sát thương ngẫu nhiên
            Player.GetComponent<PlayerHealthSystem>().TakeDamage(DamageDone); // lấy component và nhả sát thương
        }
    }
    void EventAfterAttack(){ // sau khi tung đòn đánh sẽ đổi lại trạng thái
        attacked = false;
        state = State.Moving;
        agent.speed = BasicSpeed; // đặt tốc độ
    }
    void EventPlayAttackSound(){ // chơi tiếng động khi tấn công
        soundManager.Play("Swing" + Random.Range(0,3)); 
    }
    void EventPlayCracking(){ // chơi tiếng động khi tấn công
        soundManager.Play("Crack" + Random.Range(0,3)); 
    }
    void EventFootSteps(){ // chơi tiếng động khi bước đi
        soundManager.Play("Step" + Random.Range(0,3));
    }
    void EventSpecialReset(){ // đặt lại đòn đánh đặc biệt với số thời gian đã cho
        Invoke(nameof(SpecialReset), Random.Range(SpecialCooldown * 0.8f, SpecialCooldown));
    }
    void SpecialReset(){ // đặt lại đòn đánh đặc biệt
        CanSpecial = true;
    }    
    void EVLookat(){
        transform.LookAt(Target.transform);
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