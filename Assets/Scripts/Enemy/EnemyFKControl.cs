using UnityEngine.AI; // cần để có thể điều khiển AI
using UnityEngine;
using System.Collections; // cần cho động tác nhảy tới
public class EnemyFKControl : MonoBehaviour
{
    // chỉ áp dụng cho Fallen Knight
    // không có trạng thái đứng chờ hoặc lang thang
    // dùng enum để chỉ định trạng thái hiện tại của AI
    // khá dễ để né đòn đánh
    enum State { StandFirm, Moving }
    State state; // lấy trạng thái của AI
    public EntityUseStats EStats; // cần để lấy tốc độ di chuyển
    public LayerMask PlayerLayer; // cần cho việc thực hiện di chuyển, gây sát thương cho người chơi
    public Transform attackpointset; // điểm tấn công và phạm vi tấn công (đặt tầm đánh nhỏ hơn tầm gây sát thương)
    public float AttackRange, SpecialCooldown, JumpTime; // phạm vi tấn công / thời gian hồi của đòn đặc biệt / thời gian nhảy tới
    [SerializeField][Range(0.5f, 1f)] float SizeMinScale;
    [SerializeField][Range(1f, 2f)] float SizeMaxScale;
    public bool DrawlAttackRange; // vẽ tầm tấn công, ects
    bool attacked, CanUseSpecial, CanRevive; // đã tấn công hay chưa / có thể dùng đặc biệt
    float BasicSpeed, Distant; // tốc độ cơ bản, khoảng cách của mục tiêu
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
        agent.speed = BasicSpeed;

        Target = playerHealthSystem.transform; // lấy điểm sẽ nhìn vào
        soundManager.Play("Spawning"); // chơi âm thanh khi sinh ra
        state = State.StandFirm; // đặt trạng thái khi xuất hiện
        // đặt lại thời gian hồi đòn đặc biệt
        CanUseSpecial = false;
        CanRevive = true;
        EhealthSystem.HealthSetting(EStats.UseHealthMin, EStats.UseHealthMax); // đặt máu
        Invoke(nameof(EventSpecialReset), Random.Range(SpecialCooldown * 0.8f, SpecialCooldown * 1.2f));
    }
    void LateUpdate() {
        // đổi trạng thái tùy theo tình huống
        switch (state) {
            default:
            case State.StandFirm:
                // trạng thái trống
            break;

            case State.Moving: // đuổi theo
                StartChase();
            break;
        }
        // xem tình trạng còn sống hay không
        
        if (!EhealthSystem.Alive && CanRevive){ // cơ hội hồi sinh
            animator.SetTrigger("Revive"); // chơi hoạt ảnh
            CanRevive = false;
            EhealthSystem.CanTakeDamage = false;
            EhealthSystem.Alive = true;
            Stational();
        } else if(!EhealthSystem.Alive && !CanRevive){ // xác nhận chết
            Ded();
        }
    }
    void StartChase(){
        // lấy cự ly                        điểm đầu                điểm cuối
        Distant = Vector3.Distance(transform.position, Target.position);
        // nếu ở quá xa sẽ đổi sang đòn đặc biệt
        if (Distant > 10f && CanUseSpecial){
            CanUseSpecial = false; // tắt điều kiện
            animator.SetFloat("Walk", 0); // dừng chơi hoạt ảnh
            animator.SetTrigger("Jump"); // chơi hoạt ảnh
            Stational();
        }
        // nếu không dùng được đặc biệt sẽ bước tới bình thường
        if (Distant > 2.1f){ // chạy tới
            animator.SetFloat("Walk", 1); // chơi hoạt ảnh
            agent.SetDestination(Target.position); // đặt điểm đến và chạy tới
        } else { // tấn công
            animator.SetFloat("Walk", 0); // tắt hoạt ảnh
            Stational();
            if (playerHealthSystem.Alive && !attacked){
                animator.SetTrigger("Attack" + Random.Range(0, 2)); // tấn công
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
        // thu thập thông tin từ đòn đánh với :     đểm đánh                tầm đánh       lớp lấy thông tin (layermask)
        Collider[] HitPlayer = Physics.OverlapSphere(attackpointset.position, AttackRange * 1.5f, PlayerLayer);
        
        foreach(Collider Player in HitPlayer){ // gây sát thương cho người chơi nếu trúng
            float DamageDone = Random.Range(EStats.UseMinDmg, EStats.UseMaxDmg + 1); // gây 1 lượng sát thương ngẫu nhiên
            Player.GetComponent<PlayerHealthSystem>().TakeDamage(DamageDone); // lấy component và nhả sát thương
        }
    }
    void EventJumpAttack(){ // đòn tấn công
        // thu thập thông tin từ đòn đánh với :          đểm đánh         tầm đánh   lớp lấy thông tin (layermask)
        Collider[] HitPlayer = Physics.OverlapSphere(transform.position, AttackRange * 2f, PlayerLayer);
        
        foreach(Collider Player in HitPlayer){ // gây sát thương cho người chơi nếu trúng
            float DamageDone = Random.Range(EStats.UseMinDmg, EStats.UseMaxDmg + 1) / 2; // gây 1 lượng sát thương ngẫu nhiên
            Player.GetComponent<PlayerHealthSystem>().TakeDamage(DamageDone); // lấy component và nhả sát thương
        }
        soundManager.Play("Explotion");
    }
    void EventSpecialReset(){ // đặt lại đòn đánh đặc biệt với số thời gian đã cho
        Invoke(nameof(SpecialReset), Random.Range(SpecialCooldown * 0.8f, SpecialCooldown));
    }
    void SpecialReset(){ // đặt lại đòn đánh đặc biệt
        CanUseSpecial = true;
    }
    void EventEnableTrail(){
        if(trails) trails.emitting = true;
    }
    void EventDisableTrail(){
        if(trails) trails.emitting = false;
    }
    void EventRefreshHealth(){ // làm mới thanh máu
        EhealthSystem.Revive();
    }
    void EventJumpAt(){
        StartCoroutine(StartJumping()); // bắt đầu nhảy
        soundManager.Play("Jumping");
    }
    void EventLookAtPlayer(){
        transform.LookAt(Target);
    }
    void EventAfterAttack(){ // sau khi tung đòn đánh sẽ kiểm tra vị trí của 2 bên
        attacked = false;
        state = State.Moving;
    }
    void EventPlayAttackSound(){ // chơi tiếng động khi tấn công
        soundManager.Play("Swing" + Random.Range(0,3)); 
    }
    void EventFootSteps(){ // chơi tiếng động khi bước đi
        soundManager.Play("Step" + Random.Range(0,3));
    }
    void EVLookat(){
        transform.LookAt(Target.transform);
    }
    IEnumerator StartJumping(){
        Vector3 CurPos = transform.position; // lấy vị trí từ lần trước
        Vector3 ToPos = Target.position + (Vector3.one * Random.Range(0.8f,1.2f));
        float Elaps = 0f; // theo dõi thời gian
        while (Elaps < JumpTime){
            transform.LookAt(Target);
            Elaps += Time.deltaTime; // tăng theo thời gian
            transform.position = Vector3.Lerp(CurPos, ToPos, Elaps / JumpTime); // bay tới vị trí
            yield return null;
        }
        transform.position = ToPos; // điểm cuối
    }
    void OnDrawGizmos() {
        if (DrawlAttackRange){
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, AttackRange * 3); // hiện tầm
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackpointset.position, AttackRange); // hiện tầm đánh
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackpointset.position, AttackRange * 1.5f); // hiện tầm gây sát thương
        }
    }
}