﻿using Devil.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public partial class FStateMachineMono : MonoBehaviour
    {
        protected FStateMachine m_StateMachine;
        [SerializeField]
        protected bool m_UseRealTime;
        [SerializeField]
        [Range(0,100)]
        protected int m_StackSize = 10;

        [HideInInspector]
        [SerializeField]
        protected FiniteState[] m_States;

        [HideInInspector]
        [SerializeField]
        protected FiniteStateTransition[] m_Transitions;

        [SerializeField]
        protected MonoBehaviour m_OtherImplement;

        [SerializeField]
        protected bool m_IsSubStateMachine;
        public bool IsSubStateMachine { get { return m_IsSubStateMachine; } set { m_IsSubStateMachine = value; } }
        public float LastDeltaTime { get; private set; }

        protected void InitFSM()
        {
            if (m_StateMachine == null)
            {
                m_StateMachine = new FStateMachine(name, m_States.Length, m_Transitions.Length);
                m_StateMachine.MaxStackSize = m_StackSize;
                MonoBehaviour impl = m_OtherImplement ?? this;
                for (int i = 0; i < m_States.Length; i++)
                {
                    FiniteState state = m_States[i];
                    if (state.Init(impl))
                    {
                        m_StateMachine.AddState(state, state.m_IsDefaultState);
                    }
                }
                for (int i = 0; i < m_Transitions.Length; i++)
                {
                    FiniteStateTransition trans = m_Transitions[i];
                    if (trans.Init(impl))
                    {
                        m_StateMachine.AddTransition(trans.m_FromState, trans.m_ToState, trans);
                    }
                }
            }
        }

        protected virtual void Awake()
        {
            //GenerateFSM();
            InitFSM();
        }

        protected virtual void Start()
        {
            if (!m_IsSubStateMachine)
                m_StateMachine.OnBegin();
        }

        protected virtual void Update()
        {
            LastDeltaTime = m_UseRealTime ? Time.unscaledDeltaTime : Time.deltaTime;
            m_StateMachine.OnTick(LastDeltaTime);
        }

        protected virtual void OnDestroy()
        {
            if (m_StateMachine != null)
            {
                m_StateMachine.Release();
                m_StateMachine = null;
            }
        }

        public FStateMachine FSM
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return m_StateMachine;
#endif
                InitFSM();
                return m_StateMachine;
            }
        }

        public int StateLength { get { return m_States == null ? 0 : m_States.Length; } }

        public int TransitionLength { get { return m_Transitions == null ? 0 : m_Transitions.Length; } }

        public string CurrentStateName { get { return m_StateMachine == null ? null : m_StateMachine.CurrentStateName; } }

        public float CurrentStateTime { get { return m_StateMachine == null ? 0 : m_StateMachine.CurrentStateTime; } }

        public float TotalRuntime { get { return m_StateMachine == null ? 0 : m_StateMachine.TotalRuntime; } }

        public int CurrentStateTicks { get { return m_StateMachine == null ? 0 : m_StateMachine.CurrentStateTicks; } }

        public int TotalTicks { get { return m_StateMachine == null ? 0 : m_StateMachine.TotalTicks; } }

        public MonoBehaviour Implement
        {
            get { return m_OtherImplement ?? this; }
        }

        //void GenerateFSM()
        //{
        //    List<FiniteState> states = new List<FiniteState>();
        //    List<FiniteStateTransition> transitions = new List<FiniteStateTransition>();
        //    var impl = Implement;
        //    var mtds = impl.GetType().GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        //    FStateAttribute stat;
        //    FStateTransitionAttribute tran;
        //    for(int i = 0; i < mtds.Length; i++)
        //    {
        //        var ret = mtds[i].ReturnType;
        //        var stats = ret == typeof(void) ? mtds[i].GetCustomAttributes(typeof(FStateAttribute), true) : null;
        //        if (stats != null)
        //        {
        //            for (int j = 0; j < stats.Length; j++)
        //            {
        //                stat = (FStateAttribute)stats[j];
        //                var s = GlobalUtil.Find(states, (x) => x.m_StateName == stat.Name);
        //                if (s == null)
        //                {
        //                    s = new FiniteState();
        //                    s.m_StateName = stat.Name;
        //                    states.Add(s);
        //                }
        //                if ((stat.Event & EStateEvent.OnBegin) != 0)
        //                    s.m_BeginMethod = mtds[i].Name;
        //                if ((stat.Event & EStateEvent.OnTick) != 0)
        //                    s.m_TickMethod = mtds[i].Name;
        //                if ((stat.Event & EStateEvent.OnEnd) != 0)
        //                    s.m_EndMethod = mtds[i].Name;
        //                s.m_IsDefaultState |= stat.IsDefault;
        //                s.m_KeepInStack |= stat.KeepInStack;
        //                s.m_UseSubState |= stat.IsSubState;
        //            }
        //        }
        //        var trans = ret == typeof(bool) ? mtds[i].GetCustomAttributes(typeof(FStateTransitionAttribute), true) : null;
        //        if(trans != null)
        //        {
        //            for(int j = 0;j < trans.Length; j++)
        //            {
        //                tran = (FStateTransitionAttribute)trans[j];
        //                FiniteStateTransition t = new FiniteStateTransition();
        //                t.m_FromState = tran.From;
        //                t.m_ToState = tran.To;
        //                t.m_ConditionMethod = mtds[i].Name;
        //                transitions.Add(t);
        //            }
        //        }
        //    }
        //    m_States = states.ToArray();
        //    m_Transitions = transitions.ToArray();
        //}
    }
}