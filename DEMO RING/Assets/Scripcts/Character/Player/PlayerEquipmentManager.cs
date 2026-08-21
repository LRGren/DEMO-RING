using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class PlayerEquipmentManager : CharacterEquipmentManager
{
    PlayerManager player;

    [Header("Weapon Slots")]
    public WeaponModelInstantiationSlot rightHandWeaponSlot;
    public WeaponModelInstantiationSlot leftHandWeaponSlot;
    public WeaponModelInstantiationSlot leftHandShieldSlot;
    public WeaponModelInstantiationSlot backSlot;

    [Header("Weapon Managers")]
    [SerializeField] WeaponManager rightHandWeaponManager;
    [SerializeField] WeaponManager leftHandWeaponManager;

    [Header("Weapon Models")]
    public GameObject rightWeaponModel;
    public GameObject leftWeaponModel;

    [Header("General Equipment Models")]
    public GameObject hatsObject;
    [HideInInspector] public GameObject[] hats;
    public GameObject hoodsObject;
    [HideInInspector] public GameObject[] hoods;
    public GameObject faceCoversObject;
    [HideInInspector] public GameObject[] faceCovers;
    public GameObject helmetAccessoriesObject;
    [HideInInspector] public GameObject[] helmetAccessories;
    public GameObject backAccessoriesObject;
    [HideInInspector] public GameObject[] backAccessories;
    public GameObject hipAccessoriesObject;
    [HideInInspector] public GameObject[] hipAccessories;
    public GameObject rightShoulderObject;
    [HideInInspector] public GameObject[] rightShoulders;
    public GameObject rightElbowObject;
    [HideInInspector] public GameObject[] rightElbows;
    public GameObject rightKneeObject;
    [HideInInspector] public GameObject[] rightKnees;
    public GameObject leftShoulderObject;
    [HideInInspector] public GameObject[] leftShoulders;
    public GameObject leftElbowObject;
    [HideInInspector] public GameObject[] leftElbows;
    public GameObject leftKneeObject;
    [HideInInspector] public GameObject[] leftKnees;

    [Header("Male Equipment Models")]
    public GameObject fullHelmetObject;        // 全盔
    [HideInInspector] public GameObject[] fullHelmets;
    public GameObject fullBodyObject;             // 躯干
    [HideInInspector] public GameObject[] bodies;
    public GameObject rightUpperArmObject;     // 右上臂
    [HideInInspector] public GameObject[] rightUpperArms;
    public GameObject rightLowerArmObject;     // 右前臂
    [HideInInspector] public GameObject[] rightLowerArms;
    public GameObject leftUpperArmObject;      // 左上臂
    [HideInInspector] public GameObject[] leftUpperArms;
    public GameObject leftLowerArmObject;      // 左前臂
    [HideInInspector] public GameObject[] leftLowerArms;
    public GameObject hipsObject;              // 髋部
    [HideInInspector] public GameObject[] hips;
    public GameObject rightHandObject;         // 右手
    [HideInInspector] public GameObject[] rightHands;
    public GameObject leftHandObject;          // 左手
    [HideInInspector] public GameObject[] leftHands;
    public GameObject rightLegObject;          // 右腿
    [HideInInspector] public GameObject[] rightLegs;
    public GameObject leftLegObject;           // 左腿
    [HideInInspector] public GameObject[] leftLegs;

    [Header("Female Equipment Models")]
    public GameObject femaleFullHelmetObject;        // 全盔
    [HideInInspector] public GameObject[] femaleFullHelmets;
    public GameObject femaleFullBodyObject;             // 躯干
    [HideInInspector] public GameObject[] femaleBodies;
    public GameObject femaleRightUpperArmObject;     // 右上臂
    [HideInInspector] public GameObject[] femaleRightUpperArms;
    public GameObject femaleRightLowerArmObject;     // 右前臂
    [HideInInspector] public GameObject[] femaleRightLowerArms;
    public GameObject femaleLeftUpperArmObject;      // 左上臂
    [HideInInspector] public GameObject[] femaleLeftUpperArms;
    public GameObject femaleLeftLowerArmObject;      // 左前臂
    [HideInInspector] public GameObject[] femaleLeftLowerArms;
    public GameObject femaleHipsObject;              // 髋部
    [HideInInspector] public GameObject[] femaleHips;
    public GameObject femaleRightHandObject;         // 右手
    [HideInInspector] public GameObject[] femaleRightHands;
    public GameObject femaleLeftHandObject;          // 左手
    [HideInInspector] public GameObject[] femaleLeftHands;
    public GameObject femaleRightLegObject;          // 右腿
    [HideInInspector] public GameObject[] femaleRightLegs;
    public GameObject femaleLeftLegObject;           // 左腿
    [HideInInspector] public GameObject[] femaleLeftLegs;


    [Header("Debug")]
    public bool equip = false;

    protected override void Awake()
    {
        base.Awake();

        player = GetComponent<PlayerManager>();

        InitialiseWeaponSlots();

        // 初始化装备模型数组（从父物体下读取所有子物体）
        // 头部
        InitializeEquipmentModel(ref fullHelmetObject, ref fullHelmets);
        InitializeEquipmentModel(ref femaleFullHelmetObject, ref femaleFullHelmets);
        InitializeEquipmentModel(ref hatsObject, ref hats);
        InitializeEquipmentModel(ref hoodsObject, ref hoods);
        InitializeEquipmentModel(ref faceCoversObject, ref faceCovers);
        InitializeEquipmentModel(ref helmetAccessoriesObject, ref helmetAccessories);
        // 身体
        InitializeEquipmentModel(ref fullBodyObject, ref bodies);
        InitializeEquipmentModel(ref femaleFullBodyObject, ref femaleBodies);
        InitializeEquipmentModel(ref rightUpperArmObject, ref rightUpperArms);
        InitializeEquipmentModel(ref rightLowerArmObject, ref rightLowerArms);
        InitializeEquipmentModel(ref femaleRightUpperArmObject, ref femaleRightUpperArms);
        InitializeEquipmentModel(ref femaleRightLowerArmObject, ref femaleRightLowerArms);
        InitializeEquipmentModel(ref leftUpperArmObject, ref leftUpperArms);
        InitializeEquipmentModel(ref leftLowerArmObject, ref leftLowerArms);
        InitializeEquipmentModel(ref femaleLeftUpperArmObject, ref femaleLeftUpperArms);
        InitializeEquipmentModel(ref femaleLeftLowerArmObject, ref femaleLeftLowerArms);
        InitializeEquipmentModel(ref hipsObject, ref hips);
        InitializeEquipmentModel(ref femaleHipsObject, ref femaleHips);
        // 手部
        InitializeEquipmentModel(ref rightHandObject, ref rightHands);
        InitializeEquipmentModel(ref leftHandObject, ref leftHands);
        InitializeEquipmentModel(ref femaleRightHandObject, ref femaleRightHands);
        InitializeEquipmentModel(ref femaleLeftHandObject, ref femaleLeftHands);
        // 腿部
        InitializeEquipmentModel(ref rightLegObject, ref rightLegs);
        InitializeEquipmentModel(ref leftLegObject, ref leftLegs);
        InitializeEquipmentModel(ref femaleRightLegObject, ref femaleRightLegs);
        InitializeEquipmentModel(ref femaleLeftLegObject, ref femaleLeftLegs);
        // 配件
        InitializeEquipmentModel(ref rightShoulderObject, ref rightShoulders);
        InitializeEquipmentModel(ref leftShoulderObject, ref leftShoulders);
        InitializeEquipmentModel(ref rightElbowObject, ref rightElbows);
        InitializeEquipmentModel(ref leftElbowObject, ref leftElbows);
        InitializeEquipmentModel(ref rightKneeObject, ref rightKnees);
        InitializeEquipmentModel(ref leftKneeObject, ref leftKnees);
        InitializeEquipmentModel(ref backAccessoriesObject, ref backAccessories);
        InitializeEquipmentModel(ref hipAccessoriesObject, ref hipAccessories);
    }

    #region Equipment
    private void InitializeEquipmentModel(ref GameObject equipmentParent, ref GameObject[] equipmentModels)
    {
        // 父物体未在 Inspector 中赋值时跳过，避免空引用
        if (equipmentParent == null)
        {
            equipmentModels = new GameObject[0];
            return;
        }

        List<GameObject> equipmentModelList = new List<GameObject>();
        foreach (Transform child in equipmentParent.transform)
        {
            equipmentModelList.Add(child.gameObject);
        }
        equipmentModels = equipmentModelList.ToArray();
    }

    void Update()
    {
        if (equip)
        {
            equip = false;

            EquipArmor();
        }
    }

    public void EquipArmor()
    {
        LoadHeadEquipment(player.playerInventoryManager.headEquipment);

        LoadBodyEquipment(player.playerInventoryManager.bodyEquipment);

        LoadHandEquipment(player.playerInventoryManager.handEquipment);

        LoadLegEquipment(player.playerInventoryManager.legEquipment);
    }

    // Equipment Slots
    public void LoadHeadEquipment(HeadEquipmentItem headEquipment)
    {
        // 1. UNLOAD OLD HEAD EQUIPMENT MODELS (IF ANY)
        UnloadHeadEquipment();
        // 2. IF EQUIPMENT IS NULL SIMPLY SET EQUIPMENT IN INVENTORY TO NULL AND RETURN
        if (headEquipment == null)
        {
            if (player.IsOwner)
                player.playerNetworkManager.headEquipmentID.Value = -1;
            player.playerInventoryManager.headEquipment = null;
            return;
        }

        // 3. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW
        // 4. SET CURRENT HEAD EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
        player.playerInventoryManager.headEquipment = headEquipment;

        // 5. IF YOU NEED TO CHECK FOR HEAD EQUIPMENT TYPE TO DISABLE CERTAIN BODY FEATURES (HOODS DISABLING HAIR ECT, FULL HELMS DISABLING HEADS) DO IT NOW
        switch (headEquipment.headEquipmentType)
        {
            case HeadEquipmentType.FullHelmet:
                player.playerBodyManager.DisableHead();
                player.playerBodyManager.DisableHair();
                break;
            case HeadEquipmentType.Hood:
                player.playerBodyManager.DisableHair();
                break;
            case HeadEquipmentType.FaceCover:
                player.playerBodyManager.DisableFacialHair();
                break;
            case HeadEquipmentType.Hat:
                break;
            default:
                Debug.LogError("Unknown head equipment type.");
                break;
        }

        // 6. LOAD HEAD EQUIPMENT MODELS
        foreach (var model in headEquipment.equipmentModels)
        {
            model.LoadEquipmentModel(player, player.playerNetworkManager.isMale.Value);
        }

        // 7. CALCULATE TOTAL EQUIPMENT LOAD (WEIGHT OF ALL YOUR WORN EQUIPMENT. THIS IMPACTS ROLL SPEED AND AT EXTREME WEIGHTS, MOVEMENT SPEED)
        // 8. CALCULATE TOTAL ARMOR ABSORPTION
        player.playerStatsManager.CaculateTotalCharacterAborption();

        if (player.IsOwner)
            player.playerNetworkManager.headEquipmentID.Value = headEquipment.itemID;

    }

    public void UnloadHeadEquipment()
    {
        foreach (var model in fullHelmets)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleFullHelmets)
        {
            model.SetActive(false);
        }

        foreach (var model in hats)
        {
            model.SetActive(false);
        }

        foreach (var model in hoods)
        {
            model.SetActive(false);
        }

        foreach (var model in faceCovers)
        {
            model.SetActive(false);
        }

        foreach (var model in helmetAccessories)
        {
            model.SetActive(false);
        }

        player.playerBodyManager.EnableHead();
        player.playerBodyManager.EnableHair();
        player.playerBodyManager.EnableFacialHair();
    }

    public void LoadBodyEquipment(BodyEquipmentItem bodyEquipment)
    {
        // 1. UNLOAD OLD BODY EQUIPMENT MODELS (IF ANY)
        UnloadBodyEquipment();

        // 2. IF EQUIPMENT IS NULL SIMPLY SET EQUIPMENT IN INVENTORY TO NULL AND RETURN
        if (bodyEquipment == null)
        {
            if (player.IsOwner)
                player.playerNetworkManager.bodyEquipmentID.Value = -1;
            player.playerInventoryManager.bodyEquipment = null;
            return;
        }

        // 3. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW
        // 4. SET CURRENT BODY EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
        player.playerInventoryManager.bodyEquipment = bodyEquipment;

        // 5. IF YOU NEED TO CHECK FOR BODY EQUIPMENT TYPE TO DISABLE CERTAIN BODY FEATURES DO IT NOW
        player.playerBodyManager.DisableBody();

        // 6. LOAD BODY EQUIPMENT MODELS
        foreach (var model in bodyEquipment.equipmentModels)
        {
            model.LoadEquipmentModel(player, player.playerNetworkManager.isMale.Value);
        }

        // 7. CALCULATE TOTAL EQUIPMENT LOAD (WEIGHT OF ALL YOUR WORN EQUIPMENT. THIS IMPACTS ROLL SPEED AND AT EXTREME WEIGHTS, MOVEMENT SPEED)
        // 8. CALCULATE TOTAL ARMOR ABSORPTION
        player.playerStatsManager.CaculateTotalCharacterAborption();

        if (player.IsOwner)
            player.playerNetworkManager.bodyEquipmentID.Value = bodyEquipment.itemID;
    }

    public void UnloadBodyEquipment()
    {
        foreach (var model in bodies)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleBodies)
        {
            model.SetActive(false);
        }

        foreach (var model in rightUpperArms)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleRightUpperArms)
        {
            model.SetActive(false);
        }

        foreach (var model in leftUpperArms)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleLeftUpperArms)
        {
            model.SetActive(false);
        }

        foreach (var model in rightShoulders)
        {
            model.SetActive(false);
        }

        foreach (var model in leftShoulders)
        {
            model.SetActive(false);
        }

        foreach (var model in rightElbows)
        {
            model.SetActive(false);
        }

        foreach (var model in leftElbows)
        {
            model.SetActive(false);
        }

        player.playerBodyManager.EnableBody();
    }

    public void LoadHandEquipment(HandEquipmentItem handEquipment)
    {
        // 1. UNLOAD OLD HAND EQUIPMENT MODELS (IF ANY)
        UnloadHandEquipment();

        // 2. IF EQUIPMENT IS NULL SIMPLY SET EQUIPMENT IN INVENTORY TO NULL AND RETURN
        if (handEquipment == null)
        {
            if (player.IsOwner)
                player.playerNetworkManager.handEquipmentID.Value = -1;
            player.playerInventoryManager.handEquipment = null;
            return;
        }

        // 3. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW
        // 4. SET CURRENT HAND EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
        player.playerInventoryManager.handEquipment = handEquipment;

        // 5. IF YOU NEED TO CHECK FOR HAND EQUIPMENT TYPE TO DISABLE CERTAIN BODY FEATURES DO IT NOW
        player.playerBodyManager.DisableArms();

        // 6. LOAD HAND EQUIPMENT MODELS
        foreach (var model in handEquipment.equipmentModels)
        {
            model.LoadEquipmentModel(player, player.playerNetworkManager.isMale.Value);
        }

        // 7. CALCULATE TOTAL EQUIPMENT LOAD (WEIGHT OF ALL YOUR WORN EQUIPMENT. THIS IMPACTS ROLL SPEED AND AT EXTREME WEIGHTS, MOVEMENT SPEED)
        // 8. CALCULATE TOTAL ARMOR ABSORPTION
        player.playerStatsManager.CaculateTotalCharacterAborption();

        if (player.IsOwner)
            player.playerNetworkManager.handEquipmentID.Value = handEquipment.itemID;
    }

    public void UnloadHandEquipment()
    {
        // 停用所有手部模型
        foreach (var model in rightLowerArms)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleRightLowerArms)
        {
            model.SetActive(false);
        }

        foreach (var model in leftLowerArms)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleLeftLowerArms)
        {
            model.SetActive(false);
        }

        foreach (var model in rightHands)
        {
            model.SetActive(false);
        }

        foreach (var model in leftHands)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleRightHands)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleLeftHands)
        {
            model.SetActive(false);
        }

        player.playerBodyManager.EnableArms();
    }

    public void LoadLegEquipment(LegEquipmentItem legEquipment)
    {
        // 1. UNLOAD OLD LEG EQUIPMENT MODELS (IF ANY)
        UnloadLegEquipment();

        // 2. IF EQUIPMENT IS NULL SIMPLY SET EQUIPMENT IN INVENTORY TO NULL AND RETURN
        if (legEquipment == null)
        {
            if (player.IsOwner)
                player.playerNetworkManager.legEquipmentID.Value = -1;
            player.playerInventoryManager.legEquipment = null;
            return;
        }

        // 3. IF YOU HAVE AN "ONITEMEQUIPPED" CALL ON YOUR EQUIPMENT, RUN IT NOW
        // 4. SET CURRENT LEG EQUIPMENT IN PLAYER INVENTORY TO THE EQUIPMENT THAT IS PASSED TO THIS FUNCTION
        player.playerInventoryManager.legEquipment = legEquipment;

        // 5. IF YOU NEED TO CHECK FOR LEG EQUIPMENT TYPE TO DISABLE CERTAIN BODY FEATURES DO IT NOW
        player.playerBodyManager.DisableLegs();

        // 6. LOAD LEG EQUIPMENT MODELS
        foreach (var model in legEquipment.equipmentModels)
        {
            model.LoadEquipmentModel(player, player.playerNetworkManager.isMale.Value);
        }

        // 7. CALCULATE TOTAL EQUIPMENT LOAD (WEIGHT OF ALL YOUR WORN EQUIPMENT. THIS IMPACTS ROLL SPEED AND AT EXTREME WEIGHTS, MOVEMENT SPEED)
        // 8. CALCULATE TOTAL ARMOR ABSORPTION
        player.playerStatsManager.CaculateTotalCharacterAborption();

        if (player.IsOwner)
            player.playerNetworkManager.legEquipmentID.Value = legEquipment.itemID;
    }

    public void UnloadLegEquipment()
    {
        // 停用所有腿部模型
        foreach (var model in hips)
        {
            model.SetActive(false);
        }

        foreach (var model in rightLegs)
        {
            model.SetActive(false);
        }

        foreach (var model in leftLegs)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleRightLegs)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleLeftLegs)
        {
            model.SetActive(false);
        }

        foreach (var model in femaleHips)
        {
            model.SetActive(false);
        }

        foreach (var model in rightKnees)
        {
            model.SetActive(false);
        }

        foreach (var model in leftKnees)
        {
            model.SetActive(false);
        }

        player.playerBodyManager.EnableLegs();
    }


    #endregion

    private void InitialiseWeaponSlots()
    {
        WeaponModelInstantiationSlot[] weaponSlots = GetComponentsInChildren<WeaponModelInstantiationSlot>();

        foreach (var weapon in weaponSlots)
        {
            if (weapon.weaponSlot == WeaponModelSlot.RightHandSlot)
            {
                rightHandWeaponSlot = weapon;
            }
            else if (weapon.weaponSlot == WeaponModelSlot.LeftHandWeaponSlot)
            {
                leftHandWeaponSlot = weapon;
            }
            else if (weapon.weaponSlot == WeaponModelSlot.LeftHandShieldSlot)
            {
                leftHandShieldSlot = weapon;
            }
            else if (weapon.weaponSlot == WeaponModelSlot.BackSlot)
            {
                backSlot = weapon;
            }
        }
    }

    #region Main Hand Weapon and Off Hand Weapon
    public void LoadWeaponsOnBothHands()
    {
        LoadRightWeapon();
        LoadLeftWeapon();
    }

    // Right Weapon
    public void LoadRightWeapon()
    {
        if (player.playerInventoryManager.currentRightHandWeapon != null)
        {
            rightHandWeaponSlot.UnloadWeapon();

            rightWeaponModel = Instantiate(player.playerInventoryManager.currentRightHandWeapon.weaponModel);
            rightHandWeaponSlot.PlaceWeaponIntoSlot(rightWeaponModel);

            rightHandWeaponManager = rightWeaponModel.GetComponentInChildren<WeaponManager>();
            rightHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);

            player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentRightHandWeapon.weaponAnimator);
        }
    }

    public void SwitchRightWeapon()
    {
        if (!player.IsOwner)
            return;

        player.playerAnimatorManager.PlayerTargetActionAnimation("Swap_Right_Weapon_01", false, false, true, true);

        //确认是否有其他武器，如果有，切换武器
        //如果没有，切换到空手
        WeaponItem selectedWeapon = null;

        player.playerInventoryManager.rightWeaponIndex++;

        if (player.playerInventoryManager.rightWeaponIndex < 0 || player.playerInventoryManager.rightWeaponIndex > 2)
        {
            player.playerInventoryManager.rightWeaponIndex = 0;
            int weaponCount = 0;
            WeaponItem firstWeapon = null;
            int firstWeaponPosition = 0;

            for (int i = 0; i < player.playerInventoryManager.weaponsInRightHand.Length; i++)
            {
                if (player.playerInventoryManager.weaponsInRightHand[i].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    weaponCount++;
                    if (firstWeapon == null)
                    {
                        firstWeapon = player.playerInventoryManager.weaponsInRightHand[i];
                        firstWeaponPosition = i;
                    }
                }
            }

            if (weaponCount <= 1)
            {
                player.playerInventoryManager.rightWeaponIndex = -1;
                selectedWeapon = WorldItemDatabase.Instance.unarmedWeapon;
                player.playerNetworkManager.currentRightHandWeaponID.Value = selectedWeapon.itemID;
            }
            else
            {
                player.playerInventoryManager.rightWeaponIndex = firstWeaponPosition;
                player.playerNetworkManager.currentRightHandWeaponID.Value = firstWeapon.itemID;
            }


            return;
        }

        foreach (WeaponItem weaponItem in player.playerInventoryManager.weaponsInRightHand)
        {
            //Debug.Log(player.playerInventoryManager.rightWeaponIndex + " and " + player.playerInventoryManager.weaponsInRightHand.Length);
            if (player.playerInventoryManager.weaponsInRightHand[player.playerInventoryManager.rightWeaponIndex].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
            {
                selectedWeapon = player.playerInventoryManager.weaponsInRightHand[player.playerInventoryManager.rightWeaponIndex];

                //需要分配武器ID到网络上使得客户端能够正确加载武器模型
                player.playerNetworkManager.currentRightHandWeaponID.Value = player.playerInventoryManager.weaponsInRightHand[player.playerInventoryManager.rightWeaponIndex].itemID;

                return;
            }
        }

        if (selectedWeapon == null && player.playerInventoryManager.rightWeaponIndex <= 2)
        {
            SwitchRightWeapon();
        }
    }

    // Left Weapon 
    public void LoadLeftWeapon()
    {
        if (player.playerInventoryManager.currentLeftHandWeapon != null)
        {
            if (leftHandWeaponSlot.currentWeapon != null)
                leftHandWeaponSlot.UnloadWeapon();

            if (leftHandShieldSlot.currentWeapon != null)
                leftHandShieldSlot.UnloadWeapon();

            leftWeaponModel = Instantiate(player.playerInventoryManager.currentLeftHandWeapon.weaponModel);

            switch (player.playerInventoryManager.currentLeftHandWeapon.weaponType)
            {
                case WeaponType.Weapon:
                    leftHandWeaponSlot.PlaceWeaponIntoSlot(leftWeaponModel);
                    break;
                case WeaponType.Shield:
                    leftHandShieldSlot.PlaceWeaponIntoSlot(leftWeaponModel);
                    break;
                default:
                    Debug.LogError("Unknown weapon type for left hand weapon.");
                    break;
            }

            leftHandWeaponManager = leftWeaponModel.GetComponentInChildren<WeaponManager>();
            leftHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
        }
    }

    public void SwitchLeftWeapon()
    {
        if (!player.IsOwner)
            return;

        player.playerAnimatorManager.PlayerTargetActionAnimation("Swap_Left_Weapon_01", false, false, true, true);

        //确认是否有其他武器，如果有，切换武器
        //如果没有，切换到空手
        WeaponItem selectedWeapon = null;

        player.playerInventoryManager.leftWeaponIndex++;

        if (player.playerInventoryManager.leftWeaponIndex < 0 || player.playerInventoryManager.leftWeaponIndex > 2)
        {
            player.playerInventoryManager.leftWeaponIndex = 0;
            int weaponCount = 0;
            WeaponItem firstWeapon = null;
            int firstWeaponPosition = 0;

            for (int i = 0; i < player.playerInventoryManager.weaponsInLeftHand.Length; i++)
            {
                if (player.playerInventoryManager.weaponsInLeftHand[i].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
                {
                    weaponCount++;
                    if (firstWeapon == null)
                    {
                        firstWeapon = player.playerInventoryManager.weaponsInLeftHand[i];
                        firstWeaponPosition = i;
                    }
                }
            }

            if (weaponCount <= 1)
            {
                player.playerInventoryManager.leftWeaponIndex = -1;
                selectedWeapon = WorldItemDatabase.Instance.unarmedWeapon;
                player.playerNetworkManager.currentLeftHandWeaponID.Value = selectedWeapon.itemID;
            }
            else
            {
                player.playerInventoryManager.leftWeaponIndex = firstWeaponPosition;
                player.playerNetworkManager.currentLeftHandWeaponID.Value = firstWeapon.itemID;
            }


            return;
        }

        foreach (WeaponItem weaponItem in player.playerInventoryManager.weaponsInLeftHand)
        {
            //Debug.Log(player.playerInventoryManager.leftWeaponIndex + " and " + player.playerInventoryManager.weaponsInLeftHand.Length);
            if (player.playerInventoryManager.weaponsInLeftHand[player.playerInventoryManager.leftWeaponIndex].itemID != WorldItemDatabase.Instance.unarmedWeapon.itemID)
            {
                selectedWeapon = player.playerInventoryManager.weaponsInLeftHand[player.playerInventoryManager.leftWeaponIndex];

                //需要分配武器ID到网络上使得客户端能够正确加载武器模型
                player.playerNetworkManager.currentLeftHandWeaponID.Value = player.playerInventoryManager.weaponsInLeftHand[player.playerInventoryManager.leftWeaponIndex].itemID;

                return;
            }
        }

        if (selectedWeapon == null && player.playerInventoryManager.leftWeaponIndex <= 2)
        {
            SwitchLeftWeapon();
        }
    }
    #endregion

    #region Two Hand Weapon
    // Two Hand Weapon
    public void UnTwoHandWeapon()
    {
        //更新动画
        player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentRightHandWeapon.weaponAnimator);

        //判断之前双持的是什么武器
        if (player.playerInventoryManager.currentRightHandWeapon == player.playerInventoryManager.currentTwoHandedWeapon)
        {
            //说明之前双持的是右手武器，恢复左手武器
            player.playerAnimatorManager.PlayerTargetActionAnimation("TH_Back_Left_Weapon_01", player.isPerformingAction, false, true, true);
        }
        else if (player.playerInventoryManager.currentLeftHandWeapon == player.playerInventoryManager.currentTwoHandedWeapon)
        {
            //说明之前双持的是左手武器，恢复右手武器
            player.playerAnimatorManager.PlayerTargetActionAnimation("TH_Back_Right_Weapon_01", player.isPerformingAction, false, true, true);
        }

        //去除力量加成

        //恢复非双持武器
        if (player.playerInventoryManager.currentLeftHandWeapon.weaponType == WeaponType.Weapon)
        {
            leftHandWeaponSlot.PlaceWeaponIntoSlot(leftWeaponModel);
        }
        else if (player.playerInventoryManager.currentLeftHandWeapon.weaponType == WeaponType.Shield)
        {
            leftHandShieldSlot.PlaceWeaponIntoSlot(leftWeaponModel);
        }

        rightHandWeaponSlot.PlaceWeaponIntoSlot(rightWeaponModel);

        //damage collider更新
        rightHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
        leftHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
    }

    public void TwoHandRightWeapon()
    {
        //检查是否可以双持
        if (player.playerInventoryManager.currentRightHandWeapon == WorldItemDatabase.Instance.unarmedWeapon)
        {

            //如果RETURNING 或者 NOT TWO HANDING，直接RESET BOOL
            if (player.IsOwner)
            {
                player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                player.playerNetworkManager.isTwoHandingRightWeapon.Value = false;
            }

            return;
        }

        //更新动画
        player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentRightHandWeapon.weaponAnimator);

        player.playerAnimatorManager.PlayerTargetActionAnimation("TH_Back_Left_Weapon_01", player.isPerformingAction, false, true, true);

        //将non two hand weapon放在back slot
        player.playerEquipmentManager.backSlot.PlaceWeaponModelInUnequipedSlot(leftWeaponModel, player.playerInventoryManager.currentLeftHandWeapon.weaponClass, player);

        //将two hand weapon放在right hand slot
        rightHandWeaponSlot.PlaceWeaponIntoSlot(rightWeaponModel);

        rightHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
        leftHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
    }

    public void TwoHandLeftWeapon()
    {
        //检查是否可以双持
        if (player.playerInventoryManager.currentLeftHandWeapon == WorldItemDatabase.Instance.unarmedWeapon)
        {

            //如果RETURNING 或者 NOT TWO HANDING，直接RESET BOOL
            if (player.IsOwner)
            {
                player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                player.playerNetworkManager.isTwoHandingLeftWeapon.Value = false;
            }

            return;
        }

        //更新动画
        player.playerAnimatorManager.UpdateAnimatorController(player.playerInventoryManager.currentLeftHandWeapon.weaponAnimator);

        player.playerAnimatorManager.PlayerTargetActionAnimation("TH_Back_Right_Weapon_01", player.isPerformingAction, false, true, true);

        //将non two hand weapon放在back slot
        player.playerEquipmentManager.backSlot.PlaceWeaponModelInUnequipedSlot(rightWeaponModel, player.playerInventoryManager.currentRightHandWeapon.weaponClass, player);

        //将two hand weapon放在right hand slot
        rightHandWeaponSlot.PlaceWeaponIntoSlot(leftWeaponModel);

        rightHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentRightHandWeapon);
        leftHandWeaponManager.SetWeaponDamage(player, player.playerInventoryManager.currentLeftHandWeapon);
    }
    #endregion

    // Damage Colliders
    public void OpenDamageCollider()
    {
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            rightHandWeaponManager.meleeWeaponDamageCollider.EnableDamageCollider();
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager.currentRightHandWeapon.wooshes));
        }
        else if (player.playerNetworkManager.isUsingLeftHand.Value)
        {
            leftHandWeaponManager.meleeWeaponDamageCollider.EnableDamageCollider();
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.ChooseRandomSFXFromArray(player.playerInventoryManager.currentLeftHandWeapon.wooshes));
        }

        //双手共持
    }

    public void CloseDamageCollider()
    {
        if (player.playerNetworkManager.isUsingRightHand.Value)
        {
            rightHandWeaponManager.meleeWeaponDamageCollider.DisableDamageCollider();
        }
        else if (player.playerNetworkManager.isUsingLeftHand.Value)
        {
            leftHandWeaponManager.meleeWeaponDamageCollider.DisableDamageCollider();
        }
        //双手共持
    }

}
