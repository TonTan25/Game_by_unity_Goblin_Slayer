using System.Collections;
using UnityEngine.AI; // cần để có thể điều khiển AI
using UnityEngine;
public class GrimDrakeControl : MonoBehaviour {
    // chỉ áp dụng cho tử lâu long
    // không có trạng thái đứng chờ hoặc lang thang
    // tấn công
    [SerializeField][Range(0.5f, 2f)] float MinSize, MaxSize;
    [SerializeField] float RangeAtk, RangeCD, AOERad, AOECD, MeleeRange, StopRange, BeamRad, JumpTime; //
    //              tầm tấn công xa|   gần   |    diện rộng| thời gian hồi của đòn đặc biệt
    [SerializeField] float MinDMG, MaxDMG, SpearSetWaitTime;
    //                  sát thương gây ra| thời gian tạo thương
    // triệu hồi thương
    [SerializeField] GameObject OGBeamP1, OGBeamP2, OGSpear, AOE1, AOE2;
    [SerializeField] GameObject[] SpearList;
    // số lượng thương (5, 1 nguyên bản, 4 nhân bản) vị trí của thương (0 là miệng, 1 2 3 4 là phần cạnh)
    [SerializeField] Transform[] SpearPos; // vị trí tạo tầm xa
    [SerializeField] Transform AttackSet; // điểm tấn công và phạm vi tấn công. đặt tầm đánh nhỏ hơn tầm gây sát thương
    [SerializeField] EntityUseStats AGStats;
    [SerializeField] Animator AGAnimate; // thực hiện hành động và động tác
    [SerializeField] NavMeshAgent AG; // cần cho AI đi lại
    [SerializeField] EnemyHealthSystem EhealthSystem;
    [SerializeField] LayerMask TargetLayer; // cần cho việc thực hiện di chuyển, gây sát thương cho người chơi
    [SerializeField] SoundManager SoundManager; // âm thanh cho chạy, nhảy, nhận sát thương, v.v..+
    PlayerHealthSystem Target; // xác nhận đã chết hay chưa
    Coroutine LookCoroutine; // sử dụng để nhìn vật theo thời gian một cách mượt
    enum State{ StandFirm, Phase1, Phase2 } State AGstate;
    [SerializeField] bool drawGozmos; // vẽ tầm gây sát thương, tầm ra đòn
    [SerializeField] bool RangeAtked = false, MeleeAtked = false, SpearLaunch = false, BeamCharging = false; // kiểm tra điều kiện tấn công
    int SpearLeft = 0;
    float Distant; // khoảng cách vị trí giữa mẫu và người chơi
    void Start(){
        Target = FindObjectOfType<PlayerHealthSystem>();
        gameObject.transform.localScale = (Vector3.one * Random.Range(MinSize, MaxSize));
        AGSetup();
        // chuyển trạng thái bằng Animation
        StartCoroutine(SpawnCheck());
    }
    void Update(){
        switch (AGstate){
            default:
            case State.StandFirm:

            break;
            case State.Phase1:
                Phase1Active();
            break;
            case State.Phase2:
                Phase2Active();
            break;
        }
        if (!EhealthSystem.Alive) HEDED();
        if (Input.GetKeyDown(KeyCode.Space)) EhealthSystem.TakeDamage(Random.Range(1000, 5000), false);
    }
    void Phase1Active(){
        Distant = Vector3.Distance(transform.position, Target.transform.position);
        if (Distant > StopRange){
            AG.SetDestination(Target.transform.position);
            AGAnimate.SetFloat("Walk", 1f);
        } else if(Distant < StopRange && !MeleeAtked){
            MeleeAtked = true;
            AGStation();
            AGAnimate.SetTrigger("P1 Atk " + Random.Range(1, 6 + 1));
        }
        if (Distant < RangeAtk && !MeleeAtked && !RangeAtked){
            MeleeAtked = true;
            RangeAtked = true;
            AGStation();
            AGAnimate.SetTrigger("P1 Range " + Random.Range(1, 2 + 1));
        }
    }
    void Phase2Active(){
        Distant = Vector3.Distance(transform.position, Target.transform.position);
        if (Distant > StopRange){
            AG.SetDestination(Target.transform.position);
            AGAnimate.SetFloat("Walk", 1f);
        } else if(Distant < StopRange && !MeleeAtked){
            MeleeAtked = true;
            AGStation();
            AGAnimate.SetTrigger("P2 Atk " + Random.Range(1, 2 + 1));
        }
        if (Distant < RangeAtk && !MeleeAtked && !RangeAtked){
            MeleeAtked = true;
            RangeAtked = true;
            AGStation();
            AGAnimate.SetTrigger("P2 Range " + Random.Range(1, 4 + 1));
        }
    }
    void AGSetup(){
        MeleeAtked = false;
        AG.speed = Random.Range(AGStats.UseSpeed * .8f, AGStats.UseSpeed * 1.1f);
        EhealthSystem.HealthSetting(AGStats.UseHealthMin, AGStats.UseHealthMax);
        MinDMG = AGStats.UseMinDmg;
        MaxDMG = AGStats.UseMaxDmg;
        AGAnimate.SetTrigger("P1 Spawn");
        AGStation();
    }
    void CheckPhase(){ // kiểm tra trạng thái
        if (EhealthSystem.ECurrHP > EhealthSystem.EMaxHP * 0.4f) AGstate = State.Phase1;
        else AGstate = State.Phase2;
    }
// đứng cố định
    void AGStation(){ // cố định vị trí và trạng thái khi hành động
        AGAnimate.SetFloat("Walk", 0f);
        AGstate = State.StandFirm; // đổi trạng thái
        AG.SetDestination(transform.position); // đứng im
    }
    void ReRangeAtk(){
        RangeAtked = false;
    }
    void HEDED(){
        AGStation();
        EhealthSystem.CanTakeDamage = false;
        EhealthSystem.Alive = true;
        AGAnimate.SetTrigger("HEDED");
    }
// sự kiện trong động tác
    void EVSpawnDisplayDelay(){ // hiển thị lại máu
        StartCoroutine(EhealthSystem.HealthChange(EhealthSystem.ECurrHP / EhealthSystem.EMaxHP));
    }
    void EVStartLooking(){ // bắt đầu nhìn về phía người chơi
        if (LookCoroutine != null) StopCoroutine(LookCoroutine); // kiểm tra không cho đa dòng chạy cùng lúc
        LookCoroutine = StartCoroutine(LookAt());
    }
    void EVStopLooking(){ // dừng nhìn về phía người chơi
        if (LookCoroutine != null) StopCoroutine(LookCoroutine);
    }
    void EVReRange(){ // tỉ lệ tấn công tầm xa thêm 1 lần nữa
        if (Random.Range(0,10) < 2) ReRangeAtk();
        else Invoke(nameof(ReRangeAtk), Random.Range(RangeCD * 0.8f , RangeCD * 1.5f));
        MeleeAtked = false;
        CheckPhase();
    }
    void EVReMelee(){
        MeleeAtked = false;
        CheckPhase();
    }
// gọi thương
    void EVSetSpearP1(){
        StartCoroutine(InstantSpear(true));
    }
    void EVSetSpearP2(){
        StartCoroutine(InstantSpear(false));
    }
// bắn thương
    void EVLaunchSpear(){
        SpearLaunch = true;
    }
// tấn công diện rộng
    void EVSetAOEP1(){
        StartCoroutine(AOESet(true));
    }
    void EVSetAOEP2(){
        StartCoroutine(AOESet(false));
    }
// bắn laser
    void EVSetBeam1(){
        StartCoroutine(InstantBeam(OGBeamP1, Target.transform, 1.5f, BeamRad, true));
    }
    void EVSetBeam2(){
        StartCoroutine(InstantBeam(OGBeamP2, AttackSet, 0.5f, BeamRad * 1.2f, false));
    }
    void EVLaunchBeam(){
        BeamCharging = false;
    }
// tấn công từ dưới
    void EVGroundAtk(){
        AGAnimate.SetTrigger("P2 Ground Atk");
        // phần âm thanh khi xuất hiện sẽ đặt vào animation
    }
// đặt vị trí mới
    void EVUpdatePosition(){
        AG.Warp(Target.transform.position);
    }
// lao tới
    void EVDash(){
        StartCoroutine(DashAt());
    }
// chết
    void EVStartSink(){ // chìm xuống
        StartCoroutine(Sinking());
    }
// âm thanh
    void EVSpawnS(){
        //SoundManager.Play("Spawn" + Random.Range(1,2));
    }
    void EVBeamP1S(){ // tiếng sạc và bắn
        //SoundManager.Play("BeamP1 " + Random.Range(1,4));
    }
    void EVBeamP2S(){ // tiếng sạc và bắn
        //SoundManager.Play("BeamP2 " + Random.Range(1,4));
    }
    void EVSpearP1S(){ // tiếng triệu hồi thương
        //SoundManager.Play("Spear Call " + Random.Range(1,3));
    }
    void EVCallS(){ // tiếng triệu hồi bọn đệ
        //SoundManager.Play("Call");
    }
    void EVDieS(){
        //SoundManager.Play("Die");
    }
// gây sát thương
    public void EVNormalDmg(){ // tấn công thường
        //SoundManager.Play("AtkSound " + Random.Range(1,3));
        // thu thập thông tin từ đòn đánh với :         đểm đánh          tầm đánh  lớp lấy thông tin (layermask)
        Collider[] HitPlayer = Physics.OverlapSphere(AttackSet.position, MeleeRange, TargetLayer);
        foreach(Collider Player in HitPlayer){ // gây sát thương cho người chơi nếu trúng
            float DamageDone = Random.Range(MinDMG, MaxDMG + 1); // gây 1 lượng sát thương ngẫu nhiên
            Player.GetComponent<PlayerHealthSystem>().TakeDamage(DamageDone); // lấy component và nhả sát thương
        }
    }
// đếm thông số phụ
    IEnumerator SpawnCheck(){
        int SpawnAmount = Random.Range(1,2 + 1);
        while (EhealthSystem.ECurrHP > EhealthSystem.EMaxHP * 0.3f) yield return null;
        if (SpawnAmount == 1) FindObjectOfType<WaveControl>().SpawnEndless(9);
        else FindObjectOfType<WaveControl>().SpawnEndless(7);
        while (EhealthSystem.Alive) yield return null;
        if (SpawnAmount == 1) FindObjectOfType<WaveControl>().SpawnEndless(-9);
        else FindObjectOfType<WaveControl>().SpawnEndless(-7);
    }
    IEnumerator LookAt(){
        while (0 < 1){ // bắt đầu đổi hướng nhìn
            transform.LookAt(Target.transform);
            yield return null;
        }
    }
    IEnumerator DashAt(){ //
        Vector3 CurPos = transform.position; // lấy vị trí từ lần trước
        Vector3 ToPos = Target.transform.position;
        float Elaps = 0f; // theo dõi thời gian
        while (Elaps < JumpTime){
            transform.LookAt(ToPos);
            Elaps += Time.deltaTime; // tăng theo thời gian
            transform.position = Vector3.Lerp(CurPos, ToPos, Elaps / JumpTime); // bay tới vị trí
            yield return null;
        }
        transform.position = ToPos; // điểm cuối
    }
    IEnumerator InstantSpear(bool Phase1){
        int NewAmount = 0;
        if (Phase1) SpearLeft = 1;
        else SpearLeft = 4;
        while(NewAmount < SpearLeft){
            if (Phase1){
                SpearList[0] = Instantiate(OGSpear); // tạo thương
                SpearList[0].GetComponent<SpearControl>().SpearOn(Random.Range(MinDMG, MaxDMG +1));
                SpearList[0].transform.position = AttackSet.position;
            } else {
                SpearList[NewAmount] = Instantiate(OGSpear);
                SpearList[NewAmount].GetComponent<SpearControl>().SpearOn(Random.Range(MinDMG, MaxDMG +1));
                SpearList[NewAmount].transform.position = SpearPos[NewAmount].position;
                yield return new WaitForSeconds(SpearSetWaitTime);
            }
            NewAmount++;
        }
        SpearLaunch = false;
        while(!SpearLaunch) yield return null;
        while(NewAmount > 0){
            SpearList[NewAmount].GetComponent<SpearControl>().Charging = false;
            NewAmount--;
            yield return new WaitForSeconds(SpearSetWaitTime / 2f);
        }
    }
    IEnumerator InstantBeam(GameObject BeamOrigin, Transform LaunchPos, float WaitTime, float BeamScale, bool IsPhase1){
        BeamCharging = true;
        GameObject Beam = Instantiate(BeamOrigin);
        Beam.GetComponent<Collider>().enabled = false; // tắt khả năng gây sát thương
        Beam.transform.position = LaunchPos.position; // đặt
        Beam.GetComponent<AOEBeam>().SetDmg(Random.Range(MinDMG, MaxDMG +1f)); // đặt sát thương
        
        float Elaps = 0f, HoldTime = WaitTime;
        Beam.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); // kéo
        var BeamPartical = Beam.GetComponentInChildren<ParticleSystem>().emission;
        Vector3 ScaleCu, ScaleTo, Scale0, ScaleSet;
        if (IsPhase1){
            ScaleTo = new Vector3(BeamScale, 50f, BeamScale); // tới thẳng
            Scale0 = new Vector3(0f, 50f, 0f); // xóa
            ScaleSet = new Vector3(0.03f, 50f, 0.03f);
        } else {
            HoldTime *= 0.7f;
            ScaleTo = new Vector3(BeamScale / 1.5f, BeamScale / 1.5f, 50f); // tới hướng
            Scale0 = new Vector3(0f, 0f, 50f); // xóa
            ScaleSet = new Vector3(0.03f, 0.03f, 50f);
        }
        ScaleCu = Beam.transform.localScale;        // lấy

        while (BeamCharging){                       // chờ, nhắm
            if (IsPhase1){
                Vector3 PosSet = new Vector3(Target.transform.position.x, Target.transform.position.y - 1f, Target.transform.position.z);
                Beam.transform.position = PosSet;
            } else if (!IsPhase1){
                Beam.transform.LookAt(Target.transform.position);
            }
            Beam.transform.localScale = Vector3.Lerp(ScaleCu, ScaleSet, 15f);
            yield return null;
        }

        ScaleCu = Beam.transform.localScale;        // lấy
        Elaps = 0f;
        while (Elaps < HoldTime / 2f){              // xóa tia
            Elaps += Time.deltaTime;
            Beam.transform.localScale = Vector3.Lerp(ScaleCu, Scale0, Elaps);
            yield return null;
        }
        Beam.GetComponent<Collider>().enabled = true;
        ScaleCu = Beam.transform.localScale;
        Elaps = 0f;
        BeamPartical.enabled = true;
        while (Elaps < 1f){                         // bắn
            Elaps += Time.deltaTime * 15;
            Beam.transform.localScale = Vector3.Lerp(ScaleCu, ScaleTo, Elaps);
            if (IsPhase1) Beam.transform.Rotate(new Vector3(0,3,0));
            else Beam.transform.Rotate(new Vector3(0,3,3));
            yield return null;
        }

        Elaps = 0f;
        while (Elaps < 2f){                         // chờ
            Elaps += Time.deltaTime;
            if (IsPhase1) Beam.transform.Rotate(new Vector3(0,3,0));
            else Beam.transform.Rotate(new Vector3(0,0,3));
            yield return null;
        }

        Beam.GetComponent<Collider>().enabled = false;
        ScaleCu = Beam.transform.localScale;
        Elaps = 0f;
        while (Elaps < HoldTime){                   // xóa dần
            Elaps += Time.deltaTime;
            Beam.transform.localScale = Vector3.Lerp(ScaleCu, Scale0, Elaps);
            if (IsPhase1) Beam.transform.Rotate(new Vector3(0,3,0));
            else Beam.transform.Rotate(new Vector3(0,0,3));
            yield return null;
        }
        
        BeamPartical.enabled = false;
        BeamCharging = false;
        Destroy(Beam.gameObject, 5f); // xóa
    }
    IEnumerator AOESet(bool Phase1){
        GameObject AOE;
        if (Phase1) AOE = Instantiate(AOE1);
        else AOE = Instantiate(AOE2);

        Vector3 PosAt = new Vector3(transform.position.x, transform.position.y - 3f, transform.position.z);
        Vector3 PosTo = new Vector3(AOE.transform.position.x, transform.position.y + 3f, AOE.transform.position.z);
        AOE.transform.Rotate(new Vector3(0, Random.Range(0,180),0));
        AOE.transform.position = PosAt;
        AOE.transform.localScale = Vector3.one * AOERad;
        AOE.GetComponent<AOEBeam>().SetDmg(Random.Range(MinDMG, MaxDMG +1));
        AOE.GetComponent<MeshCollider>().enabled = true;
        float Elaps = 0f;
        while (Elaps < 1f){ // bắn gai
            Elaps += Time.deltaTime;
            AOE.transform.position = Vector3.MoveTowards(PosAt, PosTo, Elaps * 30f);
            yield return null;
        }
        // end up
        AOE.GetComponent<MeshCollider>().enabled = false;
        yield return new WaitForSeconds(2f);
        // end wait
        Elaps = 0f;
        PosAt = AOE.transform.position;
        PosTo = (new Vector3(AOE.transform.position.x,-3f,AOE.transform.position.z) - AOE.transform.position).normalized;
        while (Elaps < 4f){
            Elaps += Time.deltaTime;
            AOE.transform.position += PosTo * Elaps * Time.deltaTime;
            yield return null;
        }
        Destroy(AOE, 2f);
    }
    IEnumerator Sinking(){
        Vector3 PrePos = transform.position; // thông số của lần chạy code trước
        Vector3 PosTo = PrePos - new Vector3(0, 0.7f, 0);
        float Elaps = 0; // cần cho tính thời gian trôi qua
        while (Elaps < 10f){
            Elaps += Time.deltaTime; // tăng số theo thời gian
            transform.position = Vector3.MoveTowards(PrePos, PosTo, Elaps / 10f); // chìm dần
            yield return null; // kết thúc chuỗi lệnh
        }
        transform.position = PosTo;
        Destroy(gameObject, 20f);
    }
// vẽ phạm vi
    void OnDrawGizmos(){
        if (drawGozmos){
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, StopRange); // tầm dừng lại
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, RangeAtk); // tầm đánh xa
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, AOERad); // tầm đánh gần
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(AttackSet.position, MeleeRange); // tầm đánh diện rộng
        }
    }
}