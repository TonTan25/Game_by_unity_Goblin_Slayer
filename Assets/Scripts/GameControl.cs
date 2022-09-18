using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
public class GameControl : MonoBehaviour { // kiểm soát trò chơi trong khi chơi
// vật phẩm và hiển thị số lượng đã thu thập
    public int[] CurPickedAmount; // số lượng đã nhặt của mỗi loại
    public Text[] TextPauseBoardAmount; // hiển thị số lượng đã nhặt của mỗi loại
    // 0 = đợt, 1 = vàng, 2 = máu, 3 = thể lực, 4 = trang bị
    public int CurSlot = 0, Curmap; // số vị trí hiển thị hiện tại
// hệ thống và khác
    [SerializeField] GameObject[] ItemSlot; // hiển thị hình ảnh và số lượng
    [SerializeField] Animator BackgroundAnimate; // hoạt ảnh cho màn hình
// người chơi
    [SerializeField] BPCR_BasicPlayerControlRigid PlayerControl; // điều khiển di chuyển
    [SerializeField] CamsControl camsControl; // điều khiển góc nhìn
    [SerializeField] PlayerCombatSystem CombatSystem; // điều khiển trang bị
    [SerializeField] WaveControl waveControl; // đã qua bao nhiêu đợt quái
    AchiveIGTrack achiveIGTrack;
    // bộ đếm có sẵn, thay đổi khi nhặt vật phẩm
    void Start(){
        achiveIGTrack = GetComponent<AchiveIGTrack>();
        PlayerControl = FindObjectOfType<BPCR_BasicPlayerControlRigid>();
        camsControl = FindObjectOfType<CamsControl>();
        CombatSystem = FindObjectOfType<PlayerCombatSystem>();
        GetComponent<MouseCursorUnlock>().Lock();
        PlayerControling(false);
        BackgroundAnimate.SetTrigger("Blank");
        for (int i = 0; i < ItemSlot.Length; i++) ItemSlot[i].SetActive(false);
        for (int i = 0; i < TextPauseBoardAmount.Length; i++) TextPauseBoardAmount[i].text = "0";
        if (PlayerPrefs.GetInt("CurMode") == 1){
            Invoke(nameof(GameBegin), 2f);
            StartCoroutine(TimePlayed());
        } else if(PlayerPrefs.GetInt("CurMode") == 2){
            BackgroundAnimate.SetTrigger("FadeOutBlank");
            PlayerControling(true);
        }
    }
    IEnumerator TimePlayed(){ // bộ đếm thời gian đã sống sót
        PlayerHealthSystem healthSystem = FindObjectOfType<PlayerHealthSystem>();
        float TimePlayed = 0;
        while(healthSystem.Alive){
            TimePlayed += Time.deltaTime;
            yield return null;
        }
        int MinSurvive = 0, SecSurvive = 0;
        // timeplayed = 6000
        while (TimePlayed >= 60){
            TimePlayed -= 60;
            MinSurvive ++;
            yield return null;
        }
        SecSurvive = ((int)TimePlayed);
        if (PlayerPrefs.GetInt("CL") == 0) PlayerPrefs.SetString("PlayTime", MinSurvive + " phút " + SecSurvive + " giây.");
        else PlayerPrefs.SetString("PlayTime", MinSurvive + " Min " + SecSurvive + " sec.");
        PlayerPrefs.Save();
    }
    public void GameBegin(){
        BackgroundAnimate.SetTrigger("FadeOutBlank");
        PlayerControling(true);
        waveControl.SetInstantSpawn();
        CombatSystem.Switching();
    }
    public void GameOvers(){ // hết game
        if (PlayerPrefs.GetInt("CurMode") != 2){
            BackgroundAnimate.SetTrigger("Die");
            PlayerControling(false);
            GetComponent<MouseCursorUnlock>().UnLock();
            Invoke(nameof(EndGameSceneChange), 5.5f);
        } else {

        }
    }
    void EndGameSceneChange(){
        if (PlayerPrefs.GetInt("CurMode") == 1){ // lưu chỉ số đã thu thập
            PlayerPrefs.SetInt("WaveDP", waveControl.CurWave);
            PlayerPrefs.SetInt("GoldDP", CurPickedAmount[1]);
            PlayerPrefs.SetInt("HealthDP", CurPickedAmount[2]);
            PlayerPrefs.SetInt("StaDP", CurPickedAmount[3]);
            PlayerPrefs.Save();
        }
        SceneManager.LoadScene(4);
    }
    public void PlayerControling(bool Active){
        if (PlayerPrefs.GetInt("CurMode") != 2){   
            if (!Active) FindObjectOfType<WeaponsSystem>().WPanimate.speed = 0.001f;
            CombatSystem.enabled = Active;
            CombatSystem.WPSystem.enabled = Active;
            PlayerControl.enabled = Active;
            camsControl.enabled = Active;
        } else {
            if (!Active) FindObjectOfType<TestWeapons>().WPAnimate.speed = 0.001f;
            else FindObjectOfType<TestWeapons>().WPAnimate.speed = 1f;
            PlayerControl.enabled = Active;
            camsControl.enabled = Active;
        }
    }
    // hiển thị vật phẩm vừa nhặt
    public void ItemPickPop(int Indext, Sprite NewImg, string NewName, int NewAmount, Color NewTextColor){
        ItemSlot[CurSlot].GetComponentInChildren<Image>().sprite = NewImg;
        ItemSlot[CurSlot].GetComponentInChildren<Text>().text = NewName + " x " + NewAmount;
        ItemSlot[CurSlot].GetComponentInChildren<Text>().color = NewTextColor;
        ItemGained(Indext, NewAmount); // thêm số lượng
        PlayerPrefs.SetInt("Achivement " + Indext, + NewAmount); // cộng thêm số lượng mới
        StopCoroutine(ItemDisplayed(CurSlot)); // dừng lại nếu trùng
        StartCoroutine(ItemDisplayed(CurSlot)); // chạy bộ đếm mới
        // cố định lại hàng chờ
        if (CurSlot < 5) CurSlot++;
        if (CurSlot >= 5) CurSlot = 0;
    }
    IEnumerator ItemDisplayed(int Slot){
        ItemSlot[Slot].SetActive(true);
        yield return new WaitForSeconds(5f); // đợi 5 giây
        ItemSlot[Slot].SetActive(false);
        CurSlot = 0;
    }
    //hiển thị số lượng thay đổi
    public void ItemGained(int Indext, int ItemAdd){
        CurPickedAmount[Indext] += ItemAdd;
        TextPauseBoardAmount[Indext].text = CurPickedAmount[Indext].ToString();
    }
}