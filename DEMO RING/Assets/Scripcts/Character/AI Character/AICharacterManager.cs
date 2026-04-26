using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICharacterManager : CharacterManager
{
    [HideInInspector] public AICharacterCombatManager aiCharacterCombatManager;
    [HideInInspector] public AIChracterNetworkManager aiCharacterNetworkManager;
    [HideInInspector] public AICharacterLocomotionManager aiCharacterLocomotionManager;

    [Header("AI State")]
    public AIState currentState;

    [Header("AI Navigation")]
    public NavMeshAgent navMeshAgent;

    [Header("AI States")]
    public IdleState idle;
    public PursueTargetState pursueTarget;


    protected override void Awake()
    {
        base.Awake();
        aiCharacterCombatManager = GetComponent<AICharacterCombatManager>();
        aiCharacterNetworkManager = GetComponent<AIChracterNetworkManager>();
        aiCharacterLocomotionManager = GetComponent<AICharacterLocomotionManager>();
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();

        //拷贝一份，不改变原始的状态对象
        idle = Instantiate(idle);
        pursueTarget = Instantiate(pursueTarget);

        currentState = idle;

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        ProcessStateMachine();
    }

    /*public void ProcessStateMachine()
    {
        AIState nextState = null;

        if (currentState != null)
        {
            nextState = currentState.Tick(this);
        }

        if(nextState != null)
        {
            currentState = nextState;
        }
    }*/
    private void ProcessStateMachine()
    {
        // 1. 使用 Null 合并操作符 (?.) 确保 currentState 不为空时才执行 Tick
        // 2. 将 Tick 的返回值（即建议的下一个状态）存入 nextState
        AIState nextState = currentState?.Tick(this);

        // 3. 只有当 Tick 返回了一个明确的“新状态”时，才进行切换
        if (nextState != null)
        {
            currentState = nextState;
        }

        navMeshAgent.transform.localPosition = Vector3.zero;
        navMeshAgent.transform.localRotation = Quaternion.identity;

        if (navMeshAgent.enabled)
        {
            Vector3 agentDestination = navMeshAgent.destination;
            float remainingDistance = Vector3.Distance(transform.position, agentDestination);

            if (remainingDistance > navMeshAgent.stoppingDistance)
            {
                aiCharacterNetworkManager.isMoving.Value = true;
            }
            else
            {
                aiCharacterNetworkManager.isMoving.Value = false;
            }
        }
        else
        {
            aiCharacterNetworkManager.isMoving.Value = false;
        }
    }
}
