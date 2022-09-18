using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class PlayerHealthSystem : MonoBehaviour {
// máu tăng giảm theo thời gian - đối với người chơi
    enum HealthMode{Normal, TField}
    HealthMode healthMode;
    BPCR_BasicPlayerControlRigid playerControlRigid;
    public Image HealthImg, StaminaImg;
    public Text HealthText, StaText;
    public float MaxHP, CurrentHP, HPResSpeed, MaxSta, CurrentSta, StaUseSpeed, StaResSpeed, DashCost, DashTime, DashCD;
//              |       máu       tốc độ hồi |      thể lực       tốc độ dùng,  tốc độ hồi |        sử dụng lướt       |
    [Range(1f, 10f)][SerializeField]float DashSpeed; // lực lướt
    [HideInInspector] public bool Alive, DashAble = false, ChargeAble = true, HealAble = false, CanTakeDamage = true; // điều kiện còn sống, lướt
    SoundManager sound; // âm thanh nhận sát thương và hồi máu
    Camera camera;
    public float CameraDashFov;
    float CamsNormalFov;
    int HealOverTimeAmount; // số lượng máu sẽ hồi phục qua thời gian (không giới hạn số lượng máu hồi)
    void Start(){
        camera = FindObjectOfType<Camera>();
        playerControlRigid = GetComponent<BPCR_BasicPlayerControlRigid>();
        sound = GetComponent<SoundManager>();
        healthMode = HealthMode.Normal; // chế độ máu mặc định
        CamsNormalFov = camera.fieldOfView;
        CameraDashFov += CamsNormalFov;
        // hiển thị
        HealthImg.fillAmount = 0f;
        StaminaImg.fillAmount = 0f;
        HealthText.text = "0 / 0";
        StaText.text = "0 / 0";
        // số thực
        CurrentSta = MaxSta;
        CurrentHP = MaxHP;
        Alive = true;
        DisplayChange(HealthImg, HealthText, MaxHP, CurrentHP);
        if(PlayerPrefs.GetInt("CurMode") != 2) Invoke(nameof(ActiveStats), 3f);
    }
    void LateUpdate(){ // cho khả năng sử dụng lướt
        // if (Input.GetKeyDown(KeyCode.L) && DashAble){ // tắt lướt
        //     DashAble = false;
        // } else if (Input.GetKeyDown(KeyCode.L) && !DashAble){ // bật lướt
        //     DashAble = true;
        // }
        StaControl();
    }
    void StaControl(){ // kiểm soát thể lực và lướt
        switch (healthMode){
            default:
            case HealthMode.Normal:
            NormalInput();
            break;
            
            case HealthMode.TField:
            TFieldInput();
            break;
        }
    }
    void NormalInput(){
        if (Input.GetKey(KeyCode.LeftShift) && CurrentSta > 0){ // chạy
            playerControlRigid.canRun = true;
            CurrentSta -= Time.deltaTime * StaUseSpeed;
            DisplayChange(StaminaImg, StaText, MaxSta, CurrentSta);
        } else if (CurrentSta < MaxSta){
            playerControlRigid.canRun = false;
            CurrentSta += Time.deltaTime * StaResSpeed;
            DisplayChange(StaminaImg, StaText, MaxSta, CurrentSta);
        }
        // lướt
        if (DashAble && Input.GetKeyDown(KeyCode.LeftControl) && CurrentSta > DashCost){
            ChargeAble = false; // tắt sạc nặng lượng
            DashAble = false; // xác nhận lướt
            playerControlRigid.canRun = false; // tắt khả năng chạy
            CurrentSta -= DashCost; // tiêu thể lực khi sử dụng
            StartCoroutine(StatsChange(CurrentSta, MaxSta, StaminaImg, StaText));
            StartCoroutine(ActiveDash());
        }
    }
    void TFieldInput(){
        if (Input.GetKey(KeyCode.LeftShift)) playerControlRigid.canRun = true; // giữ chạy
        else playerControlRigid.canRun = false; // thả chạy
        
        if (DashAble && Input.GetKeyDown(KeyCode.LeftControl) && CurrentSta > DashCost * 2){
            ChargeAble = false; // tắt sạc nặng lượng
            DashAble = false; // xác nhận lướt
            playerControlRigid.canRun = false; // tắt khả năng chạy
            CurrentSta -= (DashCost * 2); // tiêu thể lực khi sử dụng
            StartCoroutine(ActiveDash());
            StartCoroutine(StatsChange(CurrentSta, MaxSta, StaminaImg, StaText));
        }
// hồi phục chỉ số theo thời gian
        if (ChargeAble && CurrentSta < MaxSta){
            playerControlRigid.canRun = false;
            CurrentSta += Time.deltaTime * StaResSpeed;
            DisplayChange(StaminaImg, StaText, MaxSta, CurrentSta);
        }
        if (HealAble && CurrentHP < MaxHP){
            CurrentHP += Time.deltaTime * HPResSpeed;
            DisplayChange(HealthImg, HealthText, MaxHP, CurrentHP);
        }
    }
// xem lại chỉ số và làm mới lại
    public void ActiveStats(){ // tăng chỉ số theo thời gian
        if (PlayerPrefs.GetInt("CurMode") != 2){
            StartCoroutine(StatsChange(CurrentSta, MaxSta, StaminaImg, StaText));
            StartCoroutine(StatsChange(CurrentHP, MaxHP, HealthImg, HealthText));
        } else {
            healthMode = HealthMode.TField;
            ChargeAble = true;
            HealAble = true;
            DashAble = true;
            StartCoroutine(StatsChange(CurrentSta, MaxSta, StaminaImg, StaText));
            StartCoroutine(StatsChange(CurrentHP, MaxHP, HealthImg, HealthText));
        }
    }
    // có thể gian lận được 1 lần nhận sát thương
    public void TakeDamage(float DamageTake){
        if (CanTakeDamage && Alive){
            sound.Play("TakeDamage" + Random.Range(0,3));
            CurrentHP -= ((int)DamageTake); // nhận st
            if (CurrentHP <= 0){
                CurrentHP = 0;
                HealthImg.fillAmount = 0f;
                Alive = false;
                FindObjectOfType<GameControl>().GameOvers();
            }
            StartCoroutine(StatsChange(CurrentHP, MaxHP, HealthImg, HealthText));
        }
    }
// hồi lại lượng thể lực nhất định
    public void StaRestore(int Mana){
        if (CurrentSta <= MaxSta) CurrentSta += Mana;
        if (CurrentSta > MaxSta) CurrentSta = MaxSta;
        StartCoroutine(StatsChange(CurrentSta, MaxSta, StaminaImg, StaText));
    }
// sử dụng bình máu / hồi lại lượng máu
    public void HealthRestore(float HealAmount, bool IsHPPotion){
        // kiểm tra điều kiện
        if (CurrentHP < MaxHP){
            CurrentHP += HealAmount;
            if (CurrentHP >= MaxHP) CurrentHP = MaxHP; // tránh hồi quá lượng máu đã cho
            StartCoroutine(StatsChange(CurrentHP, MaxHP, HealthImg, HealthText)); // thực hiện tăng máu
            if (IsHPPotion){
                HealOverTimeAmount += ((int)HealAmount) / 2;
                CancelInvoke(); // tránh lỗi > 2 invoke hồi máu chạy song song
                Invoke(nameof(RegenHealth), HPResSpeed); // gọi hồi máu theo thời gian
            }
        }
    }
    void RegenHealth(){ // hồi máu theo thời gian
        if (HealOverTimeAmount > 0 && CurrentHP < MaxHP){
            HealOverTimeAmount--;
            CurrentHP ++;
            DisplayChange(HealthImg, HealthText, MaxHP, CurrentHP);
            Invoke(nameof(RegenHealth), HPResSpeed); // gọi hồi máu theo thời gian
        }
    }
    void DisplayChange(Image DisplayImg, Text ImgText, float MaxStat, float CurStat){ // thay đổi hiển thị
        DisplayImg.fillAmount = CurStat / MaxStat;
        ImgText.text = CurStat.ToString("0") + " / " + MaxStat;
    }
    IEnumerator ActiveDash(){ // bắt đầu lướt - tăng tốc độ di chuyển lên một số nhất định
        sound.Play("Dash");
        CanTakeDamage = false;
        playerControlRigid.CurrSpeed *= DashSpeed; // tăng tốc độ di chuyển
        float FOV = camera.fieldOfView;
        float Elaps = 0;
        Vector3 A = new Vector3(1f, 0.8f, 1f);
        Vector3 B = new Vector3(1f, 1f, 1f);
        gameObject.transform.localScale = A;
        while (Elaps < DashTime){
            Elaps += Time.deltaTime;
            camera.fieldOfView = Mathf.Lerp(FOV, CameraDashFov, Elaps / 0.3f);
            yield return null;
        }
        playerControlRigid.CurrSpeed = playerControlRigid.BaseSpeed; // đặt lại tốc độ di chuyển
        playerControlRigid.canRun = true; // bật lại sau khi đã lướt
        CanTakeDamage = true;
        ChargeAble = true; // bật sạc năng lượng
        
        FOV = camera.fieldOfView;
        Elaps = 0;
        while (Elaps < DashTime * 1.5f){
            Elaps += Time.deltaTime;
            gameObject.transform.localScale = Vector3.Lerp(A, B, Elaps / 0.3f);
            camera.fieldOfView = Mathf.Lerp(FOV, CamsNormalFov, Elaps / 0.3f);
            yield return null;
        }
        camera.fieldOfView = CamsNormalFov;
        gameObject.transform.localScale = B;
        yield return new WaitForSeconds(DashCD); // thời gian hồi lướt
        DashAble = true;
    }
    IEnumerator StatsChange(float StatCurr, float MaxStat, Image StatImg, Text StatText){ // thay đổi thông số
        float StatPCT = StatCurr / MaxStat;
        float Elaps = 0;
        StatText.text = StatCurr.ToString("0") + " / " + MaxStat;
        while (Elaps < 0.3f){
            Elaps += Time.deltaTime; // tăng số
            StatImg.fillAmount = Mathf.Lerp(StatImg.fillAmount, StatPCT, Elaps / 0.3f); // tăng giảm theo thời gian
            yield return null;
        }
        StatImg.fillAmount = StatPCT;
    }
}