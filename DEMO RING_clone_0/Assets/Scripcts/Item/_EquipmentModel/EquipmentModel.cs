using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment Model")]
public class EquipmentModel : ScriptableObject
{
    public EquipmentModelType equipmentModelType; // 装备模型类型
    public string equipmentModelName;             // 模型名称（与场景中模型物体名匹配）
    public string femaleEquipmentModelName;       // 女性模型名称（与场景中女性模型物体名匹配）

    public void LoadEquipmentModel(PlayerManager player, bool isMale)
    {
        if (isMale)
        {
            LoadMaleModel(player);
        }
        else
        {
            LoadFemaleModel(player);
        }
    }

    private void LoadMaleModel(PlayerManager player)
    {
        switch (equipmentModelType)
        {
            case EquipmentModelType.FullHelmet:

                foreach (var model in player.playerEquipmentManager.fullHelmets)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Hat:
                foreach (var model in player.playerEquipmentManager.hats)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Hood:
                foreach (var model in player.playerEquipmentManager.hoods)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.HelmetAcessorie:
                foreach (var model in player.playerEquipmentManager.helmetAccessories)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.FaceCover:
                foreach (var model in player.playerEquipmentManager.faceCovers)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Torso:
                foreach (var model in player.playerEquipmentManager.bodies)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Back:
                foreach (var model in player.playerEquipmentManager.backAccessories)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightShoulder:
                foreach (var model in player.playerEquipmentManager.rightShoulders)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightUpperArm:
                foreach (var model in player.playerEquipmentManager.rightUpperArms)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightElbow:
                foreach (var model in player.playerEquipmentManager.rightElbows)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightLowerArm:
                foreach (var model in player.playerEquipmentManager.rightLowerArms)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightHand:
                foreach (var model in player.playerEquipmentManager.rightHands)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftShoulder:
                foreach (var model in player.playerEquipmentManager.leftShoulders)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftUpperArm:
                foreach (var model in player.playerEquipmentManager.leftUpperArms)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftElbow:
                foreach (var model in player.playerEquipmentManager.leftElbows)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftLowerArm:
                foreach (var model in player.playerEquipmentManager.leftLowerArms)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftHand:
                foreach (var model in player.playerEquipmentManager.leftHands)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Hips:
                foreach (var model in player.playerEquipmentManager.hips)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.HipsAttachment:
                foreach (var model in player.playerEquipmentManager.hipAccessories)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightLeg:
                foreach (var model in player.playerEquipmentManager.rightLegs)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightKnee:
                foreach (var model in player.playerEquipmentManager.rightKnees)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftLeg:
                foreach (var model in player.playerEquipmentManager.leftLegs)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftKnee:
                foreach (var model in player.playerEquipmentManager.leftKnees)
                {
                    if (model.gameObject.name == equipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            default:
                break;
        }
    }

    private void LoadFemaleModel(PlayerManager player)
    {
        switch (equipmentModelType)
        {
            case EquipmentModelType.FullHelmet:
                foreach (var model in player.playerEquipmentManager.femaleFullHelmets)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Hat:
                foreach (var model in player.playerEquipmentManager.hats)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Hood:
                foreach (var model in player.playerEquipmentManager.hoods)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.HelmetAcessorie:
                foreach (var model in player.playerEquipmentManager.helmetAccessories)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.FaceCover:
                foreach (var model in player.playerEquipmentManager.faceCovers)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Torso:
                foreach (var model in player.playerEquipmentManager.femaleBodies)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Back:
                foreach (var model in player.playerEquipmentManager.backAccessories)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightShoulder:
                foreach (var model in player.playerEquipmentManager.rightShoulders)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightUpperArm:
                foreach (var model in player.playerEquipmentManager.femaleRightUpperArms)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightElbow:
                foreach (var model in player.playerEquipmentManager.rightElbows)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightLowerArm:
                foreach (var model in player.playerEquipmentManager.femaleRightLowerArms)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightHand:
                foreach (var model in player.playerEquipmentManager.femaleRightHands)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftShoulder:
                foreach (var model in player.playerEquipmentManager.leftShoulders)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftUpperArm:
                foreach (var model in player.playerEquipmentManager.femaleLeftUpperArms)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftElbow:
                foreach (var model in player.playerEquipmentManager.leftElbows)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftLowerArm:
                foreach (var model in player.playerEquipmentManager.femaleLeftLowerArms)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftHand:
                foreach (var model in player.playerEquipmentManager.femaleLeftHands)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.Hips:
                foreach (var model in player.playerEquipmentManager.femaleHips)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.HipsAttachment:
                foreach (var model in player.playerEquipmentManager.hipAccessories)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightLeg:
                foreach (var model in player.playerEquipmentManager.femaleRightLegs)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.RightKnee:
                foreach (var model in player.playerEquipmentManager.rightKnees)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftLeg:
                foreach (var model in player.playerEquipmentManager.femaleLeftLegs)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            case EquipmentModelType.LeftKnee:
                foreach (var model in player.playerEquipmentManager.leftKnees)
                {
                    if (model.gameObject.name == femaleEquipmentModelName)
                    {
                        model.gameObject.SetActive(true);
                    }
                }
                break;
            default:
                break;
        }
    }

}
