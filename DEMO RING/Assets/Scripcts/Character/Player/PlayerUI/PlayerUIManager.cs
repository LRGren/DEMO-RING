using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager instance;

    [Header("Network JOIN")]
    [SerializeField] private bool startAsClient;

    [HideInInspector] public PlayerUIHudManager playerUIHudManager;
    [HideInInspector] public PlayerUIPopUpManager playerUIPopUpManager;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        playerUIHudManager = GetComponentInChildren<PlayerUIHudManager>();
        playerUIPopUpManager = GetComponentInChildren<PlayerUIPopUpManager>();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (startAsClient)
        {
            startAsClient = false;
            //只需要一个主机，作为客户要关闭自己的主机
            NetworkManager.Singleton.Shutdown();
            //作为客户登录
            NetworkManager.Singleton.StartClient();
        }
    }
}
