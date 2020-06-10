﻿using UnityEngine;

public class TouchEffect : MonoBehaviour
{
    [SerializeField] GameObject CLICK_PARTICLE = default; // PS_TouchStarを割り当てる
    [SerializeField] GameObject DRAG_PARTICLE = default;  // PS_DragStarを割り当てる

    private GameObject m_ClickParticle = default;
    private GameObject m_DragParticle = default;

    private ParticleSystem m_ClickParticleSystem = default;
    private ParticleSystem m_DragParticleSystem = default;

    // タッチ状態管理Managerの読み込み
    [SerializeField] GameObject TouchStateManagerObj;
    public TouchStateManager TouchStateManagerScript;

    private bool DragFlag;     // ドラッグしはじめのときにtrueにする（連続でParticle.Play()させないため）
    bool touch;

    //音を鳴らす
    public AudioClip SE_awa;
    AudioSource audioSource;

    private GameObject ParticleCanvas;
    // Use this for initialization
    void Start()
    {
        // フラグの初期化
        DragFlag = false;
        touch = false;

        ParticleCanvas = GameObject.Find("UICanvas");
        // パーティクルを生成
        m_ClickParticle = (GameObject)Instantiate(CLICK_PARTICLE);
        m_DragParticle = (GameObject)Instantiate(DRAG_PARTICLE);

        m_ClickParticle.transform.SetParent(ParticleCanvas.transform, false);
        m_DragParticle.transform.SetParent(ParticleCanvas.transform, false);

        // パーティクルの再生停止を制御するためコンポーネントを取得
        m_ClickParticleSystem = m_ClickParticle.GetComponent<ParticleSystem>();
        m_DragParticleSystem = m_DragParticle.GetComponent<ParticleSystem>();

        // 再生を止める（万が一急に再生されないように）
        m_ClickParticleSystem.Stop();
        m_DragParticleSystem.Stop();

        //タッチ状態管理Managerの取得
        TouchStateManagerObj = GameObject.Find("TouchStateManager");
        TouchStateManagerScript = TouchStateManagerObj.GetComponent<TouchStateManager>();

        //Componentを取得
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // パーティクルをマウスカーソルに追従させる
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 30.0f;  // Canvasより手前に移動させる
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        
        touch = TouchStateManagerScript.GetTouch();

        if (m_DragParticle != null)
        {
            m_DragParticle.transform.position = new Vector3(mousePosition.x, mousePosition.y, mousePosition.z);
        }
        //タッチされていたら
        if (touch)
        {
            // タッチ開始時
            if (TouchStateManagerScript.GetTouchPhase() == TouchPhase.Began)
            {
                Debug.Log("★を出すよ");
                m_ClickParticle.transform.position = mousePosition;
                m_ClickParticleSystem.Play();   // １回再生(ParticleSystemのLoopingにチェックを入れていないため)
            }
            // タッチ終了
            else if (TouchStateManagerScript.GetTouchPhase() == TouchPhase.Ended)
            {
                Debug.Log("タッチエフェクト停止");
                // Particleの放出を停止する
                m_ClickParticleSystem.Stop();
                m_DragParticleSystem.Stop();

                DragFlag = false;
            }
            // タッチ中
            else if(TouchStateManagerScript.GetTouchPhase() == TouchPhase.Moved)
            {
                if (!DragFlag)
                {
                    audioSource.PlayOneShot(SE_awa);
                    Debug.Log("キラキラを出すよ");
                    m_DragParticleSystem.Play();    // ループ再生(Loopingにチェックが入っている)

                    DragFlag = true;
                }

            }
        }

    }
}