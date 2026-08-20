using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyManager : MonoBehaviour
{
    [Header("Hair")]
    public GameObject hair;
    public GameObject maleFacialHair;

    [Header("Male")]
    public GameObject maleHead;
    public GameObject[] maleBody;
    public GameObject[] maleArms;
    public GameObject[] maleLegs;
    public GameObject maleEyebrows;

    [Header("Female")]
    public GameObject femaleHead;
    public GameObject[] femaleBody;
    public GameObject[] femaleArms;
    public GameObject[] femaleLegs;
    public GameObject femaleEyebrows;

    public void EnableHead()
    {
        maleHead.SetActive(true);
        femaleHead.SetActive(true);

        maleEyebrows.SetActive(true);
        femaleEyebrows.SetActive(true);
    }

    public void DisableHead()
    {
        maleHead.SetActive(false);
        femaleHead.SetActive(false);

        maleEyebrows.SetActive(false);
        femaleEyebrows.SetActive(false);
    }

    public void EnableHair()
    {
        hair.SetActive(true);
    }

    public void DisableHair()
    {
        hair.SetActive(false);
    }

    public void EnableFacialHair()
    {
        maleFacialHair.SetActive(true);
    }

    public void DisableFacialHair()
    {
        maleFacialHair.SetActive(false);
    }

    public void EnableBody()
    {
        for (int i = 0; i < maleBody.Length; i++)
        {
            maleBody[i].SetActive(true);
        }

        for (int i = 0; i < femaleBody.Length; i++)
        {
            femaleBody[i].SetActive(true);
        }
    }

    public void DisableBody()
    {
        for (int i = 0; i < maleBody.Length; i++)
        {
            maleBody[i].SetActive(false);
        }

        for (int i = 0; i < femaleBody.Length; i++)
        {
            femaleBody[i].SetActive(false);
        }
    }

    public void EnableArms()
    {
        for (int i = 0; i < maleArms.Length; i++)
        {
            maleArms[i].SetActive(true);
        }

        for (int i = 0; i < femaleArms.Length; i++)
        {
            femaleArms[i].SetActive(true);
        }
    }

    public void DisableArms()
    {
        for (int i = 0; i < maleArms.Length; i++)
        {
            maleArms[i].SetActive(false);
        }

        for (int i = 0; i < femaleArms.Length; i++)
        {
            femaleArms[i].SetActive(false);
        }
    }

    public void EnableLegs()
    {
        for (int i = 0; i < maleLegs.Length; i++)
        {
            maleLegs[i].SetActive(true);
        }

        for (int i = 0; i < femaleLegs.Length; i++)
        {
            femaleLegs[i].SetActive(true);
        }
    }

    public void DisableLegs()
    {
        for (int i = 0; i < maleLegs.Length; i++)
        {
            maleLegs[i].SetActive(false);
        }

        for (int i = 0; i < femaleLegs.Length; i++)
        {
            femaleLegs[i].SetActive(false);
        }
    }
}
