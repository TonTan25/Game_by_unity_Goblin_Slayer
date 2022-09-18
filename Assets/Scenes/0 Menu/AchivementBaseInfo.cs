using UnityEngine;
[CreateAssetMenu(fileName = "Achivement Lists", menuName = "New Achivement List")]
public class AchivementBaseInfo : ScriptableObject //sử dụng để chứa danh sách thành tựu
{
    public Achivement[] achivements; // tạo danh sách thành tựu với những thông tin bên dưới
    
    [System.Serializable] // cho phép viết dữ liệu
    public class Achivement{ // thông tin của thành tựu
        // bản tiếng việt
        public string  Indext, AchiveNameV, AchiveInfoV, AchiveCommentV;
        //        | số thứ tự |    tên    | thông tin  |   bình luận   |
        // bản tiếng anh
        public string AchiveNameE, AchiveInfoE, AchiveCommentE;
        //           |    tên    | thông tin  |   bình luận   |
        public Sprite AchiveIcon; // ảnh
        public int AchiveTotalProgress; // tiến trình
    }
}