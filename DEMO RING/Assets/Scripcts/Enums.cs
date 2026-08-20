using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums : MonoBehaviour
{
}

public enum CharacterSlot
{
    CharacterSlot_01,
    CharacterSlot_02,
    CharacterSlot_03,
    CharacterSlot_04,
    CharacterSlot_05,
    CharacterSlot_06,
    CharacterSlot_07,
    CharacterSlot_08,
    CharacterSlot_09,
    CharacterSlot_10,
    NO_SLOT
}

public enum CharacterGroup
{
    Team_01,
    Team_02,
}

public enum WeaponModelSlot
{
    RightHandSlot,
    LeftHandWeaponSlot,
    LeftHandShieldSlot,
    BackSlot,
}

public enum WeaponType
{
    Weapon,
    Shield,
}

public enum WeaponClass
{
    StraightSword,
    MediumShield,
}

public enum EquipmentModelType
{
    FullHelmet,
    Hat,
    Hood,
    HelmetAcessorie,
    FaceCover,
    Torso,
    Back,
    RightShoulder,
    RightUpperArm,
    RightElbow,
    RightLowerArm,
    RightHand,
    LeftShoulder,
    LeftUpperArm,
    LeftElbow,
    LeftLowerArm,
    LeftHand,
    Hips,
    HipsAttachment,
    RightLeg,
    RightKnee,
    LeftLeg,
    LeftKnee
}

public enum HeadEquipmentType
{
    FullHelmet, //遮住全部
    Hat,        //不遮挡
    Hood,       //遮住头发
    FaceCover   //遮住脸
}

public enum AttackType
{
    LightAttack01,
    LightAttack02,
    LightAttack03,

    HeavyAttack01,
    HeavyAttack02,

    ChargedAttack01,
    ChargedAttack02,

    RunningAttack01,
    RollingAttack01,
    BackstepAttack01,
}

public enum DamageIntensity
{
    Ping,
    Light,
    Medium,
    Heavy,
    Colossal,
}


