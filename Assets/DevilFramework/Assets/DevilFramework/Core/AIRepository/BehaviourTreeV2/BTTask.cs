﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devil.AI
{
    public class BTTask : BTNodeBase
    {
        IBTTask mTask;

        public BTTask(int id, IBTTask task) : base(id)
        {
            mTask = task;
        }

        public override BTNodeBase ChildForVisit
        {
            get { return null; }
        }

        public override void ReturnWithState(EBTTaskState state)
        {
        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            if (mTask != null)
            {
                State = mTask.OnStartTask(behaviourTree);
            }
            else
            {
                State = EBTTaskState.faild;
            }
        }

        public override void InitData(string jsonData)
        {
            if (mTask != null)
                mTask.OnInitData(jsonData);
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
            if (mTask != null)
                State = mTask.OnTaskTick(behaviourTree, deltaTime);
        }

        public override bool AbortWithSucces()
        {
            if (mTask != null)
                return mTask.AbortWithSuccess();
            else
                return false;
        }
    }

    public class BTConstTask : BTNodeBase
    {
        EBTTaskState mState;

        public BTConstTask(EBTTaskState state) : base(0) { mState = state; }

        public override BTNodeBase ChildForVisit
        {
            get { return null; }
        }

        public override void ReturnWithState(EBTTaskState state)
        {
        }

        protected override void OnTick(BehaviourTreeRunner behaviourTree, float deltaTime)
        {
        }

        protected override void OnVisit(BehaviourTreeRunner behaviourTree)
        {
            State = mState;
        }

    }
}