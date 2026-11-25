using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_Character_HP_Bar : UI_StatBar
{
    private CharacterManager character;
    private AICharacterManager aiCharacter;
    private PlayerManager player;

    [SerializeField] private bool displayCharacterName = false;
    [SerializeField] private float defaultTimeBeforeBarHides = 3f;
    [SerializeField] private float hideTimer = 0f;
    public float currentDamageTaken = 0f;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterDamageText;
    [HideInInspector] public float oldHealthValue = 0f;

    protected override void Awake()
    {
        base.Awake();

        character = GetComponentInParent<CharacterManager>();

        aiCharacter = character as AICharacterManager;
        player = character as PlayerManager;
    }

    protected override void Start()
    {
        base.Start();

        if (hideTimer <= 0f)
            gameObject.SetActive(false);
    }

    public override void SetStat(int newValue)
    {
        // A network health update can arrive before this inactive bar's Awake.
        if (!slider)
        {
            slider = GetComponent<UnityEngine.UI.Slider>();
            character = GetComponentInParent<CharacterManager>(true);
            aiCharacter = character as AICharacterManager;
            player = character as PlayerManager;
        }

        if (!slider || !character || !character.characterNetworkManager)
            return;

        if (displayCharacterName)
        {
            if (aiCharacter != null)
            {
                characterNameText.text = aiCharacter.characterName;
            }
            else if (player != null)
            {
                characterNameText.text = player.playerNetworkManager.characterName.Value.ToString();
            }
        }
        else
        {
            characterNameText.gameObject.SetActive(false);
        }

        slider.maxValue = character.characterNetworkManager.maxHealth.Value;

        currentDamageTaken = Mathf.RoundToInt(currentDamageTaken + (oldHealthValue - newValue));

        if (currentDamageTaken < 0)
        {
            currentDamageTaken = Mathf.Abs(currentDamageTaken);
            characterDamageText.text = "+ " + currentDamageTaken.ToString();
        }
        else
        {
            characterDamageText.text = "- " + currentDamageTaken.ToString();
        }

        slider.value = newValue;

        if (character.characterNetworkManager.currentHealth.Value != character.characterNetworkManager.maxHealth.Value)
        {
            hideTimer = defaultTimeBeforeBarHides;
            gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);

        if (hideTimer > 0f)
        {
            hideTimer -= Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        currentDamageTaken = 0f;
    }
}
