using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class PlayerCombatSystem : MonoBehaviour {
    // gắn vào người chơi
    // hệ thống đánh cận chiến và sử dụng kỹ năng
    // hệ thống kỹ năng cơ bản
    [SerializeField] WeaponInfo WPInfo; // lấy thông tin cho vũ khí
    [SerializeField] Transform WeaponContainer; // nơi lấy danh sách vũ khí
    [SerializeField] Image MainWPImg, SkillImg, PotsImg, PokeImg; // ảnh đại diện chính cho vũ khí đang cầm
    [SerializeField] Sprite FourSImg, PowerSImg; // ảnh cho loại kỹ năng
    [SerializeField] Text PotsText; // hiển thị số lương bình máu còn lại
    public float PotsCD, PokeCD, SkillCD; // thời gian hồi
    [SerializeField] int HealAmount, PotsLeft, MaxPots, PokeStaUse, SkillStaUse;
    //        số máu sẽ hồi lại | bình máu còn lại, tối đa | tiêu thể lực khi sử dụng kỹ năng
    // cho phép sử dụng tính năng
    public bool PotsAllowed, ActionAllowed, CanHeal;
    public bool[] WeaponsAllowed; // dùng danh sách vũ khí
    // 0 = rìu | 1 = cúp | 2 = kiếm ngắn | 3 = kiếm dài
    [Range(0, 2)]public int SkillNumb; // số tương ứng với loại kỹ năng. 0 là không, 1 là chém mạnh, 2 là chém 4 lần
    int CurrWP = 0, PreWP = 1; // nhập vũ khí hiện tại
    [SerializeField]PlayerHealthSystem playerHealthSystem; // cần khi dùng bình máu
    public WeaponsSystem WPSystem; // cần cho việt sử dụng hoạt ảnh đòn đánh
    AchiveIGTrack achiveIGTrack;
    [HideInInspector] public TrailRenderer trails;
    [SerializeField] Coroutine SkillCDSet;
    SoundManager sounds;
    void Start(){
        // lấy comps
        achiveIGTrack = FindObjectOfType<AchiveIGTrack>();
        playerHealthSystem = GetComponent<PlayerHealthSystem>();
        WPSystem = FindObjectOfType<WeaponsSystem>();
        sounds = GetComponent<SoundManager>();
        // tắt tính năng
        if (PlayerPrefs.GetInt("CurMode") != 2){
            SkillChange();
            PotsLeft = Random.Range(2,5);
            PotsText.text = PotsLeft + " / " + MaxPots; // đặt số lượng bình máu
        }
    }
    void Update(){
        SelectingWeapons();
        PlayerCombatAction();
    }
    void PlayerCombatAction(){
        // đổi kỹ năng
        if (Input.GetKeyDown(KeyCode.V) && SkillNumb != 1){
            SkillNumb = 1;
            SkillChange();
        } else if (Input.GetKeyDown(KeyCode.V) && SkillNumb != 2){
            SkillNumb = 2;
            SkillChange();
        }
        // thực hiện hành động hoặc dùng kỹ năng
        if (ActionAllowed){ 
            if (Input.GetMouseButton(0)){
                WPSystem.DoNormalAttack(CurrWP);
                ActionAllowed = false;
            }
            if (Input.GetMouseButton(1) && CurrWP > 1 && PokeImg.fillAmount == 1f && playerHealthSystem.CurrentSta > PokeStaUse){
                WPSystem.DoPokeAttack();
                ActionAllowed = false;
                StartCoroutine(SkillRefresh(PokeCD , PokeImg));
                playerHealthSystem.StaRestore(-PokeStaUse);
            }
            if (Input.GetKeyDown(KeyCode.R) && CurrWP > 1 && SkillImg.fillAmount == 1f && playerHealthSystem.CurrentSta > SkillStaUse){
                achiveIGTrack.AchivePorgress(9, true, 1);
                WPSystem.DoSkillAttack(SkillNumb);
                ActionAllowed = false;
                SkillCDSet = StartCoroutine(SkillRefresh(SkillCD, SkillImg));
                playerHealthSystem.StaRestore(-SkillStaUse);
            }
        }
        // sử dụng bình máu với điều kiện
        if (Input.GetKeyDown(KeyCode.C) && PotsAllowed && PotsImg.fillAmount == 1f && PotsLeft > 0 && playerHealthSystem.CurrentHP < playerHealthSystem.MaxHP){
            CanHeal = false;
            PotsLeft --;
            achiveIGTrack.AchivePorgress(16, true, 1);
            PotsText.text = PotsLeft + " / " + MaxPots; // đặt số lượng bình máu
            sounds.Play("UsePotion");
            StartCoroutine(SkillRefresh(PotsCD, PotsImg));
            playerHealthSystem.HealthRestore(HealAmount, true);
        }
    }
    void SelectingWeapons(){
        // đổi vũ khí với số bấm
        if (Input.GetKeyDown(KeyCode.Alpha1) && WeaponsAllowed[0]){ // dùng rìu
            CurrWP = 0;
            Switching();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && WeaponsAllowed[1]){ // dùng cúp chim
            CurrWP = 1;
            Switching();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && WeaponsAllowed[2]){ // dùng kiếm
            CurrWP = 2;
            Switching();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && WeaponsAllowed[3]){ // dùng kiếm paladin
            CurrWP = 3;
            Switching();
        }
    }
    public void Switching(){ // đổi vũ khí
        if (PreWP != CurrWP){ // lấy vũ khí đúng
            WeaponContainer.GetChild(PreWP).gameObject.SetActive(false);
            PreWP = CurrWP;
            WPSystem.WPanimate.SetTrigger("Switch");
        }
        if (PreWP == CurrWP){ // đổi thông số
            WeaponContainer.GetChild(CurrWP).gameObject.SetActive(true);
            trails = WeaponContainer.GetChild(CurrWP).gameObject.GetComponentInChildren<TrailRenderer>(); // lấy hiệu ứng chém
            MainWPImg.sprite = WPInfo.weapons[CurrWP].WeaponImg; // thay đổi ảnh
            WPSystem.SwitchWPStat(CurrWP);
        }
        if (PreWP == 2 || PreWP == 3){ // kỹ năng khả dụng khi sử dụng kiếm
            PokeImg.color = new Color(1f, 1f, 1f, 1f); // làm sáng ảnh
            SkillImg.color = new Color(1f, 1f, 1f, 1f);
        } else {
            PokeImg.color = new Color(1f, 1f, 1f, .5f); // làm tối ảnh
            SkillImg.color = new Color(1f, 1f, 1f, .5f);
        }
    }
    void SkillChange(){
        if (SkillNumb == 1) SkillImg.sprite = PowerSImg;
        else if (SkillNumb == 2) SkillImg.sprite = FourSImg;
        if (SkillCDSet != null) StopCoroutine(SkillCDSet);
        SkillCDSet = StartCoroutine(SkillRefresh(SkillCD, SkillImg));
    }
    public void PotsUpdate(){// lưu trữ số bình máu
        if (PlayerPrefs.GetInt("CurMode") != 2){
            if (PotsLeft < MaxPots){
                PotsLeft++;
                PotsText.text = PotsLeft + " / " + MaxPots; // đặt số lượng bình máu
            } else playerHealthSystem.HealthRestore(HealAmount, true); // nếu đầy túi sẽ mút luôn máu
        } else playerHealthSystem.HealthRestore(HealAmount, false);
    }
    IEnumerator SkillRefresh(float SkillCD, Image SkillImg){ // thay đổi chỉ số theo thời gian ngắn
        SkillImg.fillAmount = 0f;
        float FillPreChange = SkillImg.fillAmount;
        float Elaps = 0;
        while (Elaps < SkillCD){
            Elaps += Time.deltaTime;
            SkillImg.fillAmount = Mathf.Lerp(FillPreChange, 1f, Elaps / SkillCD);
            yield return null;
        }
        SkillImg.fillAmount = 1f;
        sounds.Play("SkillReady");
    }
}
