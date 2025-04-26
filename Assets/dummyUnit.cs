using TMPro;
using UnityEngine;

public class dummyUnit : MonoBehaviour
{
    public draftLine thePen;
    public TextMeshPro numText;
    public int draftNo;
    public bool moving;
    private float moveTime = 2f;
    public float moveProgress = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void moveIt(){
        moving = true;
        moveProgress = 0f;
        thePen.arrowImage.SetActive(false);
        //follow the line of movement constructed by thePen, point to point
        //move to the first point of the line, then move to the next point, and so on
        //point: thePen.rawPoints

        
    }
    public void resetSelf(){
        thePen.resetPen();
        moving = false;
        draftNo = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if (thePen.drafted && draftNo==0){
            globalRoundLogic.addMe(this);
            numText.text = draftNo.ToString();
        }
        if (moving){
            moveProgress += Time.deltaTime/moveTime;
            if (moveProgress >= 1f){
                moving = false;
            }
            //move to the next point of the line, then move to the next point, and so on
            //point: thePen.rawPoints
            int progressIdx = Mathf.Clamp((int)(moveProgress * thePen.rawPoints.Count), 1, thePen.rawPoints.Count-1);
            transform.position = thePen.rawPoints[progressIdx];
        }
    }
    void OnMouseDown()
    {
        if(thePen.drafted) return;
        thePen.startDrawing();
    }
}
