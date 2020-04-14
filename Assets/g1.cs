using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Valve.VR;
using System.IO;
using System;

/// <summary>
/// Gesture input by VR 
/// Pattern1：bimanual gesture recognize by shape
/// Pattern2：selected-based direction double menu
/// Pattern3：circle double menu
/// Pattern4：gesture recognize only considering direction
/// Created by zhangzigang
/// Lastest Update on 19/11/13
/// </summary>


public class g1 : MonoBehaviour {
    private SteamVR_TrackedObject trackdeObject;
    private List<Vector2> points = new List<Vector2>();
    public GameObject gestureOnScreen;
    public InputField input;
    public InputField input2;
    public string handflag;
    public int scheme = 1;//方案类型
    public string libraryToLoad;
    private LineRenderer gestureLineRenderer;
    private int vertexCount = 0;
    float timeout;
    private string message;
    private RuntimePlatform platform;
    private Vector3 virtualKeyPosition = Vector2.zero;
    private Rect drawArea;
    private string newGestureName = "";
    private GestureLibrary gl;
    private string str, str0, str1, str2, str3, str01, str13, str03, str23, str12;
    public string letter2;
    private string alphet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ.,";


    //初始化
    public void intial()
    {
        GameObject.Find("left").GetComponent<Text>().text = "";
        GameObject.Find("right").GetComponent<Text>().text = "";
        GameObject.Find("Letter").GetComponent<Text>().text = "0000";
        GameObject.Find(handflag).transform.position = new Vector3(0f, 0f, 0f);
        str = ""; str0 = ""; str1 = ""; str2 = ""; str3 = ""; str01 = ""; str13 = ""; str23 = ""; str12 = "";
        str03 = "";
    }
    void Awake()
    {//获取手柄上的这个组件
        trackdeObject = GetComponent<SteamVR_TrackedObject>();
    }
    void Start() {
        gl = new GestureLibrary(libraryToLoad);
        platform = Application.platform;
        gestureLineRenderer = gestureOnScreen.GetComponent<LineRenderer>();
        drawArea = new Rect(0, 0, Screen.width - 370, Screen.height);
        
    }

    // 每帧更新
    void Update() {
        //方案3的初始化
        if (scheme == 3)
        {
            for (int j = 0; j < 7; j++)
            {
                double jy = 2 * Math.Sin(Math.PI / 2 - Math.PI / 3.5 * j), jx = 2 * Math.Cos(Math.PI / 2 - Math.PI / 3.5 * j);
                GameObject.Find("3left" + j).transform.localPosition = new Vector3((float)jx, (float)jy, 0f);
                GameObject.Find("3left" + j).GetComponent<Text>().text = alphet.Substring(j, 1);
                GameObject.Find("3right" + j).transform.localPosition = new Vector3((float)jx, (float)jy, 0f);
                GameObject.Find("3right" + j).GetComponent<Text>().text = alphet.Substring(j+14, 1);
            }
            for (int j = 7; j < 14; j++)
            {
                double jy = 1.25 * Math.Sin(Math.PI / 2 - Math.PI / 3.5 * j), jx = 1.25 * Math.Cos(Math.PI / 2 - Math.PI / 3.5 * j);
                GameObject.Find("3left" + j).transform.localPosition = new Vector3((float)jx, (float)jy, 0f);
                GameObject.Find("3left" + j).GetComponent<Text>().text = alphet.Substring(j, 1);
                GameObject.Find("3right" + j).transform.localPosition = new Vector3((float)jx, (float)jy, 0f);
                GameObject.Find("3right" + j).GetComponent<Text>().text = alphet.Substring(j+14, 1);
            }
        }
        timeout = GameObject.Find("Time").transform.position.x;
        timeout = timeout + 0.01f;
        GameObject.Find("Time").transform.position = new Vector3(timeout, 0f, 0f);
        var device = SteamVR_Controller.Input((int)trackdeObject.index);

        if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            float rightlong = 1;
            Gesture g = new Gesture(points);
            float xielv = (points[points.Count - 1].y - points[0].y) / (points[points.Count - 1].x - points[0].x);
            if (handflag.Equals("right"))
            {
                rightlong = (points[points.Count - 1].y - points[0].y);
            }
            //距离rightlong <1 为小范围 >1为大范围 
            string str = GameObject.Find("Letter").GetComponent<Text>().text;
            //Debug.Log(str);
            string str0 = str.Substring(0, 1);
            //Debug.Log(str0);
            string str1 = str.Substring(1, 1);
            //Debug.Log(str1);
            string str2 = str.Substring(2, 1);
            //Debug.Log(str2);
            string str3 = str.Substring(3, 1);
            if (handflag.Equals("left"))
            {
                if ((xielv >= 3) || (xielv <= -3)) { Debug.Log("shu"); str0 = "1"; }
                if ((xielv >= -0.33) && (xielv <= 0.33)) { Debug.Log("heng"); str0 = "2"; }
                if ((xielv > -3) && (xielv < -0.33)) { Debug.Log("zuoxie"); str0 = "3"; }
                if ((xielv > 0.33) && (xielv < 3)) { Debug.Log("youxie"); str0 = "4"; }
            }
            else
            {
                if ((xielv >= 3) || (xielv <= -3)) { Debug.Log("shu"); str1 = "1"; }
                if ((xielv >= -0.33) && (xielv <= 0.33)) { Debug.Log("heng"); str1 = "2"; }
                if ((xielv > -3) && (xielv < -0.33)) { Debug.Log("zuoxie"); str1 = "3"; }
                if ((xielv > 0.33) && (xielv < 3)) { Debug.Log("youxie"); str1 = "4"; }
            }
            //斜率xielv 3和0.33是分界线
            Result result = g.Recognize(gl, false);
            message = result.Name + ";" + result.Score;
            Debug.Log(message);
            //scheme1开始
            if (scheme == 1) {
                if (handflag.Equals("left"))
                { str2 = result.Name; }
                else
                { str3 = result.Name; }
                str = str0 + str1 + str2 + str3;
                str03 = str0 + str3;
                str01 = str0 + str1;
                str13 = str1 + str3;
                str12 = str1 + str2;
                str23 = str2 + str3;
                GameObject.Find("Letter").GetComponent<Text>().text = "";
                GameObject.Find("Letter").GetComponent<Text>().text = str;
                //识别结果~~
                if (str23.Equals("3D")) { input.text += "G"; intial(); }//左右形状            
                if (str23.Equals("DD")) { input.text += "M"; intial(); }
                if (str23.Equals("66")) { input.text += "W"; intial(); }

                if (str03.Equals("12")) { input.text += "B"; intial(); }//左竖右形
                if (str03.Equals("14")) { input.text += "E"; intial(); }
                if (str03.Equals("16"))
                {
                    if (str1.Equals("1")) { input.text += "K"; intial(); }
                    else { input.text += "N"; intial(); }
                }
                if (str03.Equals("28")) { input.text += "J"; intial(); }
                if (str03.Equals("2D")) { input.text += "J"; intial(); }

                if (str12.Equals("61")) { input.text += "Y"; intial(); }//左形右竖
                if (str12.Equals("19")) { input.text += "Q"; intial(); }

                if ((str03.Equals("1C")) && (rightlong > 1)) { input.text += "D"; intial(); }//左竖右形
                if ((str03.Equals("1C")) && (rightlong < 1)) { input.text += "P"; intial(); }
                if ((str03.Equals("15")) && (rightlong > 1)) { input.text += "R"; intial(); }
                if ((str03.Equals("15")) && (rightlong < 1)) { input.text += "F"; intial(); }

                if ((str13.Equals("13"))) { input.text += "C"; intial(); }//仅右手
                if ((str13.Equals("23"))) { input.text += "U"; intial(); }
                if ((str3.Equals("7"))) { input.text += "I"; intial(); }
                if ((str3.Equals("9"))) { input.text += "O"; intial(); }
                if ((str3.Equals("A"))) { input.text += "S"; intial(); }
                if ((str3.Equals("B"))) { input.text += "X"; intial(); }
                if ((str3.Equals("5"))) { input.text += "Z"; intial(); }

                if ((str01.Equals("43"))) { input.text += "A"; intial(); }//左右都为竖
                if ((str01.Equals("11"))) { input.text += "H"; intial(); }
                if ((str01.Equals("12"))) { input.text += "L"; intial(); }
                if ((str01.Equals("21"))) { input.text += "T"; intial(); }
                if ((str01.Equals("34"))) { input.text += "V"; intial(); }
            }//scheme1结束
            //scheme4开始
            if (scheme == 4)
            { 
                    
                        
                        }



                //scheme2开始
                if ((scheme == 2) && (System.Math.Abs(points[points.Count - 1].y - points[0].y) + System.Math.Abs(points[points.Count - 1].x - points[0].x) > 0.2))
            {
                int direction = 0;
                //Debug.Log(System.Math.Abs(points[points.Count - 1].y - points[0].y) + System.Math.Abs(points[points.Count - 1].x - points[0].x));
                if ((xielv >= 1) || (xielv <= -1))
                {
                    if ((points[points.Count - 1].y - points[0].y) > 0) { Debug.Log("up"); direction = 0; }
                    else { Debug.Log("down"); direction = 2; }
                }
                if ((xielv > -1) && (xielv < 1))
                {
                    if ((points[points.Count - 1].x - points[0].x) < 0) { Debug.Log("left"); direction = 1; }
                    else { Debug.Log("right"); direction = 3; }
                }
                if (handflag.Equals("left"))
                {
                    //更改右手图片

                    string[] letter22 = new string[] { "ABCDE", "FGHIJ", "KLMNO", "PQRST", "UVWXY" };
                    letter2 = letter22[direction];
                    GameObject.Find("Letter").GetComponent<Text>().text = letter2;
                    for (int i = 0; i < 5; i++) {
                        GameObject.Find("right" + i).GetComponent<Text>().text = letter2.Substring(i, 1);
                    }
                }
                if (handflag.Equals("right"))
                {
                    letter2 = GameObject.Find("Letter").GetComponent<Text>().text;
                    input.text += letter2.Substring(direction, 1);
                }
            }
            points.Clear();

            //输入正确，返回时间
            if (input.text.Equals("")) ;
            else {
                if (input.text.Equals(input2.text)) {
                    Debug.Log("correct,time:" + timeout);
                    input2.text = "correct,time:" + timeout;
                    string dia = DateTime.Now.Minute.ToString();
                    FileStream fs = new FileStream("D:\\pointdata\\record" + dia + ".txt", FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(input.text + " " + timeout);
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                    input.text = ""; }
            }


        }
        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            gestureLineRenderer.SetVertexCount(0);
            vertexCount = 0;
        }
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            if (scheme == 2)
            {
                if (handflag.Equals("left"))
                {

                    string[] letter22 = new string[] { "ABCDE", "FGHIJ", "KLMNO", "PQRST", "UVWXY" };
                    letter2 = letter22[4];
                    GameObject.Find("Letter").GetComponent<Text>().text = letter2;
                    for (int i = 0; i < 5; i++)
                    {
                        GameObject.Find("right" + i).GetComponent<Text>().text = letter2.Substring(i, 1);
                    }
                }
                else
                {
                    letter2 = GameObject.Find("Letter").GetComponent<Text>().text;
                    input.text += letter2.Substring(4, 1);
                }
            }
            if (scheme == 3)
            {
                Vector2 pos = device.GetAxis();
                float slope = (pos.y) / (pos.x);
                float distance = (pos.y) * (pos.y) + (pos.x) * (pos.x);
                if (handflag.Equals("left"))
                {
                    if (pos.x > 0)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            if (slope > Math.Tan(Math.PI * 5 / 14 - k * Math.PI * 2 / 7))
                            {
                                if (distance > 0.5f)
                                {
                                    input.text += alphet.Substring(k, 1); goto cc;
                                }
                                else
                                { input.text += alphet.Substring(k+7, 1); goto cc; }
                            }
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            if (slope > Math.Tan(-Math.PI * 11 / 14 - k * Math.PI * 2 / 7))
                            {
                                if (distance > 0.5f)
                                {
                                    input.text += alphet.Substring(k + 4, 1); ; goto cc;
                                }
                                else
                                { input.text += alphet.Substring(k + 11, 1); ; goto cc; }
                            }
                        }
                        if (distance > 0.5f)
                            input.text += alphet.Substring(0, 1);
                        else
                            input.text += alphet.Substring(7, 1);
                    }
                }
                if (handflag.Equals("right"))
                {
                    if (pos.x > 0)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            if (slope > Math.Tan(Math.PI * 5 / 14 - k * Math.PI * 2 / 7))
                            {
                                if (distance > 0.5f)
                                {
                                    input.text += alphet.Substring(k+14, 1); goto cc;
                                }
                                else
                                { input.text += alphet.Substring(k + 21, 1); goto cc; }
                            }
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            if (slope > Math.Tan(-Math.PI * 11 / 14 - k * Math.PI * 2 / 7))
                            {
                                if (distance > 0.5f)
                                {
                                    input.text += alphet.Substring(k + 18, 1); ; goto cc;
                                }
                                else
                                { input.text += alphet.Substring(k + 25, 1); ; goto cc; }
                            }
                        }
                        if (distance > 0.5f)
                            input.text += alphet.Substring(14, 1);
                        else
                            input.text += alphet.Substring(21, 1);
                    }
                }
                cc:;
            }
        }
                if (device.GetTouch(SteamVR_Controller.ButtonMask.Touchpad))
                {
                    Vector2 pos = device.GetAxis();
            //方案3
            if (scheme == 3)
            {
                float slope = (pos.y) / (pos.x);
                float distance = (pos.y) * (pos.y) + (pos.x) * (pos.x);
                    for (int j = 0; j < 14; j++)
                    {
                        GameObject.Find("3"+handflag+j).GetComponent<Text>().color = Color.white;
                    }
                    if (pos.x > 0)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            if (slope > Math.Tan(Math.PI * 5 / 14 - k * Math.PI * 2 / 7))
                            {
                                if (distance > 0.5f) {
                                    GameObject.Find("3" + handflag + k).GetComponent<Text>().color = Color.red; goto cc2; }
                                else
                                { GameObject.Find("3" + handflag + (k+7)).GetComponent<Text>().color = Color.red; goto cc2; }
                            }
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            if (slope > Math.Tan(-Math.PI * 11 / 14 - k * Math.PI * 2 / 7))
                            {
                                if (distance > 0.5f)
                                {
                                    GameObject.Find("3" + handflag + + (k + 4)).GetComponent<Text>().color = Color.red; goto cc2;
                                }
                                else
                                { GameObject.Find("3" + handflag + + (k + 11)).GetComponent<Text>().color = Color.red; goto cc2; }
                            }
                        }
                        if (distance > 0.5f)
                            GameObject.Find("3" + handflag + "0").GetComponent<Text>().color = Color.red;
                        else
                            GameObject.Find("3" + handflag +"7").GetComponent<Text>().color = Color.red;
                    }
                    cc2:;
            }
            //保存
            string s = "<point x=\"" + pos.x + "\" y=\"" + (pos.y) + "\"/>";
                    string dia = DateTime.Now.Minute.ToString();
                    FileStream fs = new FileStream("D:\\pointdata\\new" + dia + ".txt", FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.WriteLine(s);
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                    //保存结束

                    points.Add(new Vector2(pos.x, pos.y));
                    if ((scheme == 1) || (scheme == 2))
                    {
                        gestureLineRenderer.SetVertexCount(++vertexCount);
                        gestureLineRenderer.SetPosition(vertexCount - 1, WorldCoordinateForGesturePoint(pos));
                    }
                }
                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
                {
                    GameObject.Find("Time").transform.position = new Vector3(0f, 0f, 0f);
                    points.Clear();
                    int random = UnityEngine.Random.Range(0, 3);
                    input.text = "";
                    string[] parm = new string[] { "GESTURE", "RECOGNIZE", "SCENE", "MATERIAL", "SCRIPT", "EDITOR", "CORE", "EXTRA", "ICON", "PREFAB" };
                    input2.text = parm[random];
                }
                if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                {
                    GameObject.Find("Time").transform.position = new Vector3(0f, 0f, 0f);
                    points.Clear();
                    int random = UnityEngine.Random.Range(0, 3);
                    input.text = "";
                    string[] parm = new string[] { "GESTURE", "RECOGNIZE", "SCENE", "MATERIAL", "SCRIPT", "EDITOR", "CORE", "EXTRA", "ICON", "PREFAB" };
                    input2.text = parm[random];
                }
            }
            private Vector3 WorldCoordinateForGesturePoint(Vector3 gesturePoint)
            {
                Vector3 worldCoordinate = new Vector3(gesturePoint.x - 2, gesturePoint.y + 2, 0);
                if (handflag.Equals("left")) {
                    worldCoordinate = new Vector3(gesturePoint.x - 3.3f, gesturePoint.y + 2, 0); }
                return worldCoordinate;
            }
        } 
