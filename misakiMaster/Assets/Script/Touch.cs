﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touch : MonoBehaviour
{
    // タッチ状態管理Managerの読み込み
    [SerializeField] GameObject TouchStateManagerObj;
    TouchStateManager TouchStateManagerScript;

    public LayerMask mask;          // 特定レイヤーのみ判定衝突を行うようにするためのマスク、Unity上で設定（TouchManagerインスペクタ内）
    private GameObject startObj;    // タッチ始点にあるオブジェクトを格納
    private GameObject endObj;      // タッチ終点にあるオブジェクトを格納
    public string currentName;      // タグ判定用のstring変数

    // 削除するスライムのリスト
    List<GameObject> removableSlimeList = new List<GameObject>();

    public float MaxDistance;

    //マネージャー読み込み======
    public GameObject managerObj;
    manager managerScript;

    //=======================亀山

    //=========================
    // 初期化処理
    //=========================
    void Start()
    {
        //タッチ状態管理Managerの取得
        TouchStateManagerObj = GameObject.Find("TouchStateManager");
        TouchStateManagerScript = TouchStateManagerObj.GetComponent<TouchStateManager>();

        managerObj = GameObject.Find("StageManager");
        managerScript = managerObj.GetComponent<manager>();
     
    }

    //=========================
    // 更新処理
    //=========================
    void Update()
    {
        // タッチされている時
        if (managerScript.isRotate == false && TouchStateManagerScript.GetTouch())
        {
            Debug.Log("タッチ開始");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (startObj == null)
            {
                // Rayが特定レイヤの物体（スライム）に衝突している場合
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                {
                    //　大スライムにRayが衝突している時
                    if (hit.collider.gameObject.CompareTag("BigSlime"))
                    {
                        Debug.Log("爆発");

                        // 処理内容はslimeControl.csのBigSlimeClickAct()の中
                        hit.collider.gameObject.GetComponent<slimeControl>().SlimeDestroy(new Vector3(0,0,0));
                    }
                    //　小、中スライムにRayがぶつかった時
                    else if (hit.collider.gameObject.CompareTag("MiddleSlime") ||
                             hit.collider.gameObject.CompareTag("SmallSlime"))
                    {
                        currentName = hit.collider.gameObject.tag;

                        // スライムオブジェクトを格納
                        startObj = hit.collider.transform.parent.gameObject;
                        endObj = hit.collider.transform.parent.gameObject;

                        // 削除対象オブジェクトリストの初期化
                        removableSlimeList = new List<GameObject>();

                        // 削除対象のオブジェクトを格納
                        PushToList(hit.collider.gameObject);

                        Debug.Log("削除対象追加");
                    }
                }
            }
            //タッチ終了時
            else if(TouchStateManagerScript.GetTouchPhase() == TouchPhase.Ended)
            {
                int remove_cnt = removableSlimeList.Count;

                if (remove_cnt == 2)
                {
               
                    //中スライムが消された場合
                    if (startObj.CompareTag("MiddleSlime"))
                    {
                        GameObject obj = (GameObject)Resources.Load("Prefab/Fields/FieldInBIg");
                        //プレハブを元に、インスタンスを生成
                        GameObject tmp=Instantiate(obj, 
                                    new Vector3
                                    (
                                       (int)(startObj.transform.position.x + endObj.transform.position.x) / 2,
                                       (int)(startObj.transform.position.y + endObj.transform.position.y) / 2,
                                       (int)(startObj.transform.position.z + endObj.transform.position.z) / 2
                                     ),
                                      Quaternion.Euler(CreateSlimeQuarternion()));
                        //生成したプレハブをFieldCenterに登録する。
                        tmp.transform.parent = GameObject.Find("FieldCenter").transform;
                        Debug.Log("終点側に大スライムを生成");
                    }
                    //小スライムが消された場合
                    else if (startObj.CompareTag("SmallSlime"))
                    {
                        GameObject obj = (GameObject)Resources.Load("Prefab/Fields/FieldInMid");
                        //プレハブを元に、インスタンスを生成
                       GameObject tmp= Instantiate(obj,
                                    new Vector3
                                    (
                                       (int)(startObj.transform.position.x + endObj.transform.position.x) / 2,
                                       (int)(startObj.transform.position.y + endObj.transform.position.y) / 2,
                                       (int)(startObj.transform.position.z + endObj.transform.position.z) / 2
                                     ),
                                      Quaternion.Euler(CreateSlimeQuarternion()));
                        //生成したプレハブをFieldCenterに登録する。
                        tmp.transform.parent = GameObject.Find("FieldCenter").transform;
                        Debug.Log("終点側に中スライムを生成");
                    }

                    GameObject.Destroy(startObj);
                    GameObject.Destroy(endObj);
                    //          startObj.GetComponent<slimeControl>().DestroyAnimation(startObj.transform.position - endObj.transform.position);

                    // スコアと消えるときの音はここ↓※これは昔つくったやーつ

                    //scoreGUI.SendMessage("AddPoint", point * remove_cnt);
                    //efxSource.Play();
                }
                // 消す対象外の時
                else
                {
                    for (int i = 0; i < remove_cnt; i++)
                    {
                        removableSlimeList[i] = null;
                    }
                }

                // リスト内のバイキンを消す
                currentName = null;
                startObj = null;
                endObj = null;
            }
            // タッチ中
            else if(TouchStateManagerScript.GetTouchPhase() == TouchPhase.Moved && startObj != null)
            {
                // Rayが特定レイヤの物体（バイキン）に衝突している場合
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                {
                    if (hit.collider != null)
                    {
                        GameObject hitObj = hit.collider.transform.parent.gameObject;

                        // 同じタグのブロックをクリック＆endObjとは別オブジェクトである時
                        if (hitObj.tag == currentName && endObj != hitObj)
                        {
                            Debug.Log("同タグの別オブジェクトが選択された");
                            // ２つのオブジェクトの距離を取得
                            float distance = Vector2.Distance(hitObj.transform.position, endObj.transform.position);

                            if (distance <= MaxDistance)
                            {
                                Debug.Log("z値を取得し比較");
                                // zが同じであれば
                                
                                if (Mathf.Floor(Mathf.Abs(startObj.transform.parent.position.z))/(MaxDistance/2) == Mathf.Floor(Mathf.Abs(hitObj.transform.parent.position.z)) /(MaxDistance/2)
                                    &&    startObj.transform.rotation.z == hitObj.transform.rotation.z)
                                {
                                    Debug.Log("削除します");

                                    // 削除対象のオブジェクトを格納
                                    endObj = hitObj;
                                    // 削除対象のオブジェクトを格納
                                    PushToList(hitObj);
                                }
                            }
                        }
                    }
                }
            }
            
        }
    }

    //==============================================================
    //　選択されているバイキンを除去リストに格納する
    //==============================================================
    void PushToList(GameObject obj)
    {
        // 除去リストに選択しているオブジェクトを追加
        removableSlimeList.Add(obj);

        // どのオブジェクトが除去リスト入りしているかわかりやすいように名前に_をつけたす
        obj.name = "_" + obj.name;
    }

    /*==================================================
     生成されるスライムの角度を
     managerに保存されているカメラ位置に対応したRotateで生成する
     ===================================================    */

    Vector3 CreateSlimeQuarternion()
    {
        //角度別スライム生成
        Vector3 compared = startObj.transform.position;
        Vector3 compare = endObj.transform.position;
        Vector3 prefRotate = new Vector3(0, 0, 0);

        if (managerScript.cameraRotate % 2 == 0)
            prefRotate.y = 0;
        else
            prefRotate.y = 90;
        
        //位置取得。
        if (Mathf.Floor(compare.x) / (MaxDistance / 2) ==
            Mathf.Floor(compared.x) / (MaxDistance / 2)) {
            //縦長スライム生成
            prefRotate.z = 90;
        } else if (Mathf.Floor(compare.y) / (MaxDistance / 2) ==
            Mathf.Floor(compared.y) / (MaxDistance / 2)) {
            //横長スライム生成
            prefRotate.z = 0;
        }
   
            prefRotate.y = startObj.transform.parent.transform.rotation.y;
        
        return prefRotate;
    }

    public Vector3 GetStartObj()
    {
        return startObj.transform.position;
    }
}
