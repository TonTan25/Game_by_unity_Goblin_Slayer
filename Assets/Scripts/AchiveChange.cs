using UnityEngine;
using UnityEngine.UI;
public class AchiveChange : MonoBehaviour {
// gắn vào mục thành tựu, có thể thay đổi theo lệnh
    [SerializeField]AchivementBaseInfo AchiveBaseInfo;
    public Image AchiveImg;
    public Text AchiveName, AchiveInfo, AchiveComment, AchiveProgres;
    public void RefreshAchive(int AchiveIndext){
        if (PlayerPrefs.GetInt("CL") == 0) { // đổi ngôn ngữ: tên, thông tin, bình luận
            AchiveName.text = AchiveBaseInfo.achivements[AchiveIndext].AchiveNameV;
            AchiveInfo.text = AchiveBaseInfo.achivements[AchiveIndext].AchiveInfoV;
            AchiveComment.text = AchiveBaseInfo.achivements[AchiveIndext].AchiveCommentV;
        } else if (PlayerPrefs.GetInt("CL") == 1){
            AchiveName.text = AchiveBaseInfo.achivements[AchiveIndext].AchiveNameE;
            AchiveInfo.text = AchiveBaseInfo.achivements[AchiveIndext].AchiveInfoE;
            AchiveComment.text = AchiveBaseInfo.achivements[AchiveIndext].AchiveCommentE;
        }
        if (PlayerPrefs.HasKey("Achivement " + AchiveIndext)){ // kiểm tra tiến trình thành tựu
            // tiến trình hoàn thành
            if (PlayerPrefs.GetInt("Achivement " + AchiveIndext) >= AchiveBaseInfo.achivements[AchiveIndext].AchiveTotalProgress){
                PlayerPrefs.SetInt("Achivement " + AchiveIndext, AchiveBaseInfo.achivements[AchiveIndext].AchiveTotalProgress);
                AchiveProgres.text = AchiveBaseInfo.achivements[AchiveIndext].AchiveTotalProgress + " / " + AchiveBaseInfo.achivements[AchiveIndext].AchiveTotalProgress;     
                GetComponent<CanvasGroup>().alpha = 1f;
                AchiveComment.enabled = true;
                PlayerPrefs.Save();
            } else {// tiến trình chưa hoàn thành
                AchiveProgres.text = PlayerPrefs.GetInt("Achivement " + AchiveIndext) + " / " + AchiveBaseInfo.achivements[AchiveIndext].AchiveTotalProgress;     
                AchiveComment.enabled = false;
                GetComponent<CanvasGroup>().alpha = 0.7f; 
            }
        } // đặt tiến trình mới
        else {
            PlayerPrefs.SetInt("Achivement " + AchiveIndext, 0);
            AchiveProgres.text = 0 + " / " + AchiveBaseInfo.achivements[AchiveIndext].AchiveTotalProgress;
        }
    }
}