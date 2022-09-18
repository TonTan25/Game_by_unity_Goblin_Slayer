using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class TestWeapons : MonoBehaviour {
    // chỉ áp dụng cho song kiếm thử nghiệm
    [SerializeField] int BaseHealth, BaseMana, BaseMinDmg, BaseMaxDmg, RDmgAdd, DDmgAdd;
    [SerializeField][Range(0,100)] int BaseCritChance, BCritChanceAdd, DCritChanceAdd;
    [SerializeField][Range(0f, 3f)] float BaseCritDmgFloat, BCritDmgFloat, DCritDmgFloat;
    [SerializeField][Range(.8f, 3f)] float BAtkSpeed, RAtkSpeed, DAtkSpeed, BAtkRange, RAtkRange, DAtkRange;
    [SerializeField][Range(0f, 2f)] float RMoveIncreaseFloat, DMoveIncreaseFloat;
    [SerializeField] int BHealthRegen, DHealthRegen, BaseManaRegen, BManaRecov, RManaRecov, DManaRecov;
    [SerializeField][Range(0f,0.2f)] float RVampic, DVampic;
    [SerializeField] float DUseTime;
    [SerializeField] Image MainImg, RSideImg, LSideImg; // ảnh chính
    [SerializeField] Sprite BImage, RImage, DImage; // ảnh hiển thị sẽ thay vào
    [SerializeField] Transform AtkSet;
    [SerializeField] LayerMask EnemyLayer;
    public Animator WPAnimate;
    SoundManager soundManager;
    [SerializeField] TrailRenderer BTrail, RTrail, CTrail;
    [SerializeField] ParticleSystem BPartical, RPartical;
    [SerializeField] bool CanDoAction, CanSeconAction, SkillReady; // xác nhận điều kiện
    enum WeaponStage {BStage, RStage, DState} // trạng thái sử dụng D state sử dụng trong couroutine
    WeaponStage WPStage; // lấy trạng thái
    // cài đặt khác
    PlayerHealthSystem PlayerHealth; // điều chỉnh ảnh hưởng thông số
    BPCR_BasicPlayerControlRigid playercontrol; // điều chỉnh ảnh hưởng thông số
    float MovementInput, DamageDeal, VampicAmount;
    bool Crit;
    int CurrBFirstAtk, ManaRestore;
    void Start(){ // sắp đặt
        PlayerPrefs.SetInt("CurMode", 2);
        PlayerPrefs.Save();

        playercontrol = FindObjectOfType<BPCR_BasicPlayerControlRigid>(); // cài tốc độ di chuyển
        PlayerHealth = FindObjectOfType<PlayerHealthSystem>(); // cài máu và hồi phục
        StartCoroutine(DualDisplayCheck());
        WPStage = WeaponStage.BStage; // đặt chế độ cơ bản
        Invoke(nameof(Set), 2f);
    }
    void Set(){
        // điều chỉnh máu và thể lực
        PlayerHealth.CurrentHP = BaseHealth;
        PlayerHealth.CurrentSta = Random.Range(0,BaseMana);
        PlayerHealth.MaxHP = BaseHealth;
        PlayerHealth.MaxSta = BaseMana;
        // điều chỉnh hồi năng lượng và hồi máu
        PlayerHealth.HPResSpeed = BHealthRegen;
        PlayerHealth.StaUseSpeed = BaseManaRegen;
        // kích hoạt sau khi xong
        WPAnimate.enabled = true;
        PlayerHealth.ActiveStats();
        WPAnimate.SetTrigger("BDrawl");
    }
    void LateUpdate() {
        switch (WPStage){
            default:
            case WeaponStage.BStage: // dạng xanh
                BActive();
            break;
            case WeaponStage.RStage: // dạng đỏ
                RActive();
            break;
            case WeaponStage.DState: // dạng song
                DActive();
            break;
        }
    }
    void BActive(){
        // hoạt ảnh di chuyển
        MovementInput = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
        WPAnimate.SetFloat("BWalk", MovementInput);
        if(Input.GetMouseButton(0) && CanDoAction){ // tấn công chính
            CanDoAction = false;
            CurrBFirstAtk = Random.Range(1,7);
            WPAnimate.SetTrigger("BAtk " + CurrBFirstAtk);
        }
        if(Input.GetMouseButton(1) && CanSeconAction){ // tấn công phụ
            CanSeconAction = false;
            WPAnimate.SetTrigger("BSecAtk " + CurrBFirstAtk);
        }
        if(Input.GetKeyDown(KeyCode.E) && CanDoAction){ // đổi kiếm đỏ
            CanDoAction = false;
            WPAnimate.SetTrigger("SwitchBR");
            StartCoroutine(WPImgSwitch(LSideImg, RImage));
            WPStage = WeaponStage.RStage;
        }
        if(SkillReady && CanDoAction && Input.GetKeyDown(KeyCode.F)){ // đổi song kiếm
            CanDoAction = false;
            SkillReady = false;
            CurrBFirstAtk = 1;
            WPAnimate.SetTrigger("SwitchBD");
            PlayerHealth.StaRestore(-950);
            StartCoroutine(WPImgSwitch(RSideImg, DImage));
            WPStage = WeaponStage.DState;
        }
    }
    void RActive(){
        MovementInput = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
        WPAnimate.SetFloat("RWalk", MovementInput); // hoạt ảnh di chuyển
        if (Input.GetMouseButton(0) && CanDoAction){ // tấn công chính
            CanDoAction = false;
            WPAnimate.SetTrigger("RAtk " + Random.Range(1,7));
        }
        if (Input.GetMouseButton(1) && CanSeconAction){ // tấn công phụ
            CanSeconAction = false;
            WPAnimate.SetTrigger("RSecAtk " + Random.Range(1,4));
        }
        if (Input.GetKeyDown(KeyCode.E) && CanDoAction){ // đổi kiếm xanh
            CanDoAction = false;
            WPAnimate.SetTrigger("SwitchRB");
            StartCoroutine(WPImgSwitch(LSideImg, BImage));
            WPStage = WeaponStage.BStage;
        }
        if (SkillReady && CanDoAction && Input.GetKeyDown(KeyCode.F)){ // đổi song kiếm
            CanDoAction = false;
            SkillReady = false;
            CurrBFirstAtk = 2;
            WPAnimate.SetTrigger("SwitchRD");
            PlayerHealth.StaRestore(-950);
            StartCoroutine(WPImgSwitch(RSideImg, DImage));
            WPStage = WeaponStage.DState;
        }
    }
    void DActive(){
        MovementInput = Mathf.Abs(Input.GetAxis("Horizontal")) + Mathf.Abs(Input.GetAxis("Vertical"));
        WPAnimate.SetFloat("DWalk", MovementInput);
        if (Input.GetMouseButtonDown(1) && CanDoAction){
            CanDoAction = false;
            WPAnimate.SetTrigger("DAtkRB");
        }
        if (Input.GetMouseButton(0) && CanDoAction){
            CanDoAction = false;
            WPAnimate.SetTrigger("DAtk " + Random.Range(1,11));
        }
    }
    void EVStartUseD(){
        StartCoroutine(DInUse());
    }
    void EVSwitchEffect(){
        switch (WPStage){
            default:
            case WeaponStage.BStage:
                playercontrol.SpeedReFresh(playercontrol.BaseSpeed);
                PlayerHealth.HPResSpeed = BHealthRegen;
                WPAnimate.speed = BAtkSpeed;
                PlayerHealth.HealAble = true;
            break;
            case WeaponStage.RStage:
                playercontrol.SpeedReFresh(playercontrol.BaseSpeed * RMoveIncreaseFloat);
                WPAnimate.speed = RAtkSpeed;
                PlayerHealth.HealAble = false;
            break;
            case WeaponStage.DState:
                playercontrol.SpeedReFresh(playercontrol.BaseSpeed * DMoveIncreaseFloat);
                PlayerHealth.HPResSpeed = DHealthRegen;
                WPAnimate.speed = DAtkSpeed;
                PlayerHealth.HealAble = true;
            break;
        }
    }
    void BeffectOn(){
        var Set = BPartical.emission;
        BTrail.emitting = true;
        Set.enabled = true;
    }
    void BeffectOff(){
        var Set = BPartical.emission;
        BTrail.emitting = false;
        Set.enabled = false;
    }
    void CeffectOn(){
        CTrail.emitting = true;
    }
    void CeffectOff(){
        CTrail.emitting = false;
    }
    void ReffectOn(){
        var Set = RPartical.emission;
        RTrail.emitting = true;
        Set.enabled = true;
    }
    void ReffectOff(){
        var Set = RPartical.emission;
        RTrail.emitting = false;
        Set.enabled = false;
    }
    void EVReAtk(){
        CanDoAction = true;
    }
    void EVSecAtkOn(){
        CanSeconAction = true;
    }
    void EVSecAtkOff(){
        CanSeconAction = false;
    }
    void EVDmgB(){
        DoDamage(0, BCritChanceAdd, BCritDmgFloat, BManaRecov, 0, BAtkRange);
    }
    void EVDmgR(){
        DoDamage(RDmgAdd, 0, 0, RManaRecov, RVampic, RAtkRange);
    }
    void EVDmgD(){
        DoDamage(DDmgAdd, DCritChanceAdd, DCritDmgFloat, DManaRecov, DVampic, DAtkRange);
    }
    void DoDamage(int AddDmg, int AddCritChance, float AddCritDmg,int ManaRecov, float Vampic, float AtkRange){
        VampicAmount = 0;
        ManaRestore = 0;
        Collider[] TargetHit = Physics.OverlapSphere(AtkSet.position, AtkRange , EnemyLayer);
        foreach(Collider Enemy in TargetHit){ // gây sát thương nếu trúng
            DamageDeal = Random.Range(BaseMinDmg, BaseMaxDmg + 1) + AddDmg; // gây 1 lượng sát thương
            if (Random.Range(0, 100) < BaseCritChance + AddCritChance){
                DamageDeal *= (BaseCritDmgFloat + AddCritDmg);
                Crit = true;
            }
            Enemy.GetComponent<EnemyHealthSystem>().TakeDamage(DamageDeal, Crit); // gây sát thương
            ManaRestore += Random.Range(2, ManaRecov);
            VampicAmount = DamageDeal * Vampic;
        }
        PlayerHealth.HealthRestore(VampicAmount, false); // hút máu
        PlayerHealth.StaRestore(ManaRestore); // hồi năng lượng
    }
    // hoạt ảnh thay đổi ảnh hiển thị
    IEnumerator WPImgSwitch(Image SideImgs, Sprite NewImgToMain){
        float Elaps = 0;
        while (Elaps < 1f){
            Elaps += Time.deltaTime;
            MainImg.fillAmount = Mathf.Lerp(1f, 0f, Elaps);
            SideImgs.fillAmount = Mathf.Lerp(1f, 0f, Elaps);
            yield return null;
        }
        SideImgs.sprite = MainImg.sprite;
        MainImg.sprite = NewImgToMain;
        MainImg.fillAmount = 0f;
        SideImgs.fillAmount = 0f;
        Elaps = 0;
        while (Elaps < 1f){
            Elaps += Time.deltaTime;
            MainImg.fillAmount = Mathf.Lerp(0f, 1f, Elaps);
            SideImgs.fillAmount = Mathf.Lerp(0f, 1f, Elaps);
            yield return null;
        }
        MainImg.fillAmount = 1f;
        SideImgs.fillAmount = 1f;
    }
    IEnumerator DualDisplayCheck(){
        RSideImg.color = new Color(RSideImg.color.r, RSideImg.color.g, RSideImg.color.b, 0.5f);
        while (PlayerHealth.CurrentSta < 949) yield return null;
        RSideImg.color = new Color(RSideImg.color.r, RSideImg.color.g, RSideImg.color.b, 1f);
        SkillReady = true;
    }
    IEnumerator DInUse(){ // thời gian sử dụng song kiếm
        float Elaps = 0f;
        while (Elaps < DUseTime){ // cho sử dụng
            Elaps += Time.deltaTime;
            MainImg.fillAmount = Mathf.Lerp(1f, 0f, Elaps / DUseTime);
            yield return null;
        }
        CanDoAction = false;
        CanSeconAction = false;
        // đổi ảnh sau khi hết thời gian
        if(CurrBFirstAtk == 1){
            WPAnimate.SetTrigger("SwitchDB"); // đổi kiếm xanh
            StartCoroutine(WPImgSwitch(RSideImg, BImage));
            WPStage = WeaponStage.BStage;
        } else if(CurrBFirstAtk == 2){
            WPAnimate.SetTrigger("SwitchDR"); // đổi kiếm đỏ
            StartCoroutine(WPImgSwitch(RSideImg, RImage));
            WPStage = WeaponStage.RStage;
        }
        StartCoroutine(DualDisplayCheck()); // thay đổi độ sáng của ảnh
    }
}