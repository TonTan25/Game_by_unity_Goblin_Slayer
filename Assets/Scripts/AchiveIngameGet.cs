using UnityEngine.UI;
using UnityEngine;

public class AchiveIngameGet : MonoBehaviour {
    // đặt trong mục thành tựu
    public Image DisplayImg;
    public Text DisplayName, DisplayInfo;
    public Animator AchiveAnimate;
    public void AchiveDisplayActive(Sprite NewImg, string NewName, string NewInfo){
        AchiveAnimate.GetComponent<Animator>();
        DisplayImg.sprite = NewImg;
        DisplayName.text = NewName;
        DisplayInfo.text = NewInfo;
        if(PlayerPrefs.GetInt("CurMode") != 2) AchiveAnimate.SetTrigger("AchiveGet");
        else AchiveAnimate.SetTrigger("AchiveGetTField");
    }
}