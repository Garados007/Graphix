using System;
using System.Collections.Generic;
using System.Threading;

namespace Graphix.Physic
{
    /// <summary>
    /// Performs all Animations
    /// </summary>
    public class AnimationRuntime
    {
        /// <summary>
        /// Information about a single running animation
        /// </summary>
        class RunningAnimation
        {
            public AnimationGroup Group;
            public double StartTime, Timing;
            public AnimationRuntimeData Data;
            public List<AnimationEffect> Sync = new List<AnimationEffect>();
            public List<AnimationEffect> Async = new List<AnimationEffect>();
            public List<Tuple<AnimationEffect, AnimationRuntimeData>> AnimatingAsync = new List<Tuple<AnimationEffect, AnimationRuntimeData>>();
            public int SyncIndex = -1;
        }

        /// <summary>
        /// Information about the status
        /// </summary>
        class StatusPushInfo
        {
            public Status Status;
            public int Delay;
            public int? Started;
        }

        /// <summary>
        /// Status has been changed
        /// </summary>
        /// <param name="oldStatus">old Status</param>
        /// <param name="newStatus">new Status</param>
        public delegate void StatusChangedHandler(Status oldStatus, Status newStatus);

        /// <summary>
        /// List of all registered animations
        /// </summary>
        List<AnimationGroup> Animations = new List<AnimationGroup>();
        /// <summary>
        /// List of all current execution animations
        /// </summary>
        List<RunningAnimation> Running = new List<RunningAnimation>();

        /// <summary>
        /// The current implementation of the sound player
        /// </summary>
        public ISoundPlayer SoundPlayer { get; set; }

        Status currentStatus = null;
        /// <summary>
        /// Change the current Status of the ui
        /// </summary>
        public Status CurrentStatus
        {
            get => currentStatus;
            set
            {
                if (currentStatus == value) return;
                foreach (var animation in Animations)
                    foreach (var anim in animation.Activations)
                        if ((anim is StatusChange) && anim.Enabled)
                        {
                            var act = anim as StatusChange;
                            if (Status.IsSubsetOrEqualFrom(act.New, value) && Status.IsSubsetOrEqualFrom(act.Old, currentStatus))
                            {
                                ExecuteAnimation(animation);
                                break;
                            }
                        }
                StatusChanged?.Invoke(currentStatus, value);
                currentStatus = value;
            }
        }

        /// <summary>
        /// This event is called, when the ui status was changed
        /// </summary>
        public event StatusChangedHandler StatusChanged;

        /// <summary>
        /// The queue for pushed status'
        /// </summary>
        Queue<StatusPushInfo> StatusQueue = new Queue<StatusPushInfo>();

        /// <summary>
        /// Perform a change to a specific Status after some time
        /// </summary>
        /// <param name="status">the target status</param>
        /// <param name="delay">time to wait in ms after this status take place</param>
        public void PushStatus(Status status, int delay)
        {
            StatusQueue.Enqueue(new StatusPushInfo()
            {
                Status = status,
                Delay = delay,
                Started = null
            });
        }

        /// <summary>
        /// Perform all changes of the <see cref="CurrentStatus"/> 
        /// that was previosly registred with <see cref="PushStatus(Status, int)"/>
        /// </summary>
        public void FlushStatusQueue()
        {
            while (StatusQueue.Count > 0)
            {
                var info = StatusQueue.Dequeue();
                CurrentStatus = info.Status;
            }
        }

        /// <summary>
        /// Clear the queue of all changes of <see cref="CurrentStatus"/> that was
        /// registred with <see cref="PushStatus(Status, int)"/>. No changement
        /// will ocour
        /// </summary>
        public void ClearStatusQueue()
        {
            StatusQueue.Clear();
        }

        /// <summary>
        /// Check the <see cref="StatusQueue"/> for new status
        /// </summary>
        void PerformStatus()
        {
            if (StatusQueue.Count == 0) return;
            StatusPushInfo first;
            var time = Environment.TickCount;
            int dif = 0;
            do
            {
                first = StatusQueue.Peek();
                if (first.Started == null) first.Started = time - dif;
                if ((dif = time - (first.Started.Value + first.Delay)) >= 0)
                {
                    
                    CurrentStatus = first.Status;
                    StatusQueue.Dequeue();
                    first = null;
                }
            }
            while (StatusQueue.Count != 0 && first == null);
        }

        /// <summary>
        /// Register an animation
        /// </summary>
        /// <param name="animation">the animation to register</param>
        public void Register(AnimationGroup animation)
        {
            if (!Animations.Contains(animation))
            {
                Animations.Add(animation);
                foreach (var act in animation.Activations)
                    if (act is AfterAnimation)
                    {
                        var anim = act as AfterAnimation;
                        anim.Effect.HookedAnimations.Add(anim);
                    }
            }
        }

        /// <summary>
        /// Clear a registration of an animation
        /// </summary>
        /// <param name="animation">the animation to unregister</param>
        public void UnRegister(AnimationGroup animation)
        {
            Animations.Remove(animation);
            foreach (var act in animation.Activations)
                if (act is AfterAnimation)
                {
                    var anim = act as AfterAnimation;
                    anim.Effect.HookedAnimations.Remove(anim);
                }
        }

        /// <summary>
        /// Start a specified animation and add it to the running list
        /// </summary>
        /// <param name="animation">The Animation to start</param>
        /// <param name="timing">the timing multipler</param>
        public void ExecuteAnimation(AnimationGroup animation, double timing = 1)
        {
            if (timing <= 0) timing = 1;
            var running = new RunningAnimation()
            {
                Group = animation,
                StartTime = Environment.TickCount,
                Timing = timing,
                Data = new AnimationRuntimeData()
                {
                    CanAnimate = false,
                    CurrentTime = 0,
                    LastTileTime = 0,
                    Runtime = this,
                    StartTime = 0
                }
            };
            foreach (var effect in animation.Effects)
                if (effect.Async.Value) running.Async.Add(effect);
                else running.Sync.Add(effect);
            Running.Add(running);
        }

        /// <summary>
        /// Execute a single running animation
        /// </summary>
        /// <param name="animation">the running animation</param>
        void PerformAnimation(RunningAnimation animation)
        {
            var timing = animation.Timing * animation.Group.EffectTiming.Value;
            if (timing <= 0) timing = animation.Timing;
            animation.Data.CurrentTime = (Environment.TickCount - animation.StartTime) * timing * 0.001;
            for (int i = 0; i< animation.Async.Count; ++i)
            {
                var async = animation.Async[i];
                var data = new AnimationRuntimeData()
                {
                    CurrentTime = animation.Data.CurrentTime,
                    Runtime = this
                };
                async.CanStartAnimation(data);
                if (data.CanAnimate)
                {
                    animation.AnimatingAsync.Add(new Tuple<AnimationEffect, AnimationRuntimeData>(async, data));
                    animation.Async.Remove(async);
                    i--;
                }
            }
            foreach (var async in animation.AnimatingAsync)
            {
                async.Item2.CurrentTime = animation.Data.CurrentTime;
                async.Item1.Animate(async.Item2);
            }
            animation.AnimatingAsync.RemoveAll((t) => !t.Item2.CanAnimate);
            if (!animation.Data.CanAnimate)
            {
                animation.SyncIndex++;
                if (animation.SyncIndex < animation.Sync.Count)
                {
                    animation.Sync[animation.SyncIndex].CanStartAnimation(animation.Data);
                }
                if (!animation.Data.CanAnimate)
                    animation.SyncIndex--;
            }
            if (animation.Data.CanAnimate)
                animation.Sync[animation.SyncIndex].Animate(animation.Data);
            if (animation.AnimatingAsync.Count == 0 && animation.Async.Count == 0 && animation.SyncIndex >= animation.Sync.Count)
            {
                Running.Remove(animation);
                foreach (var anim in animation.Group.HookedAnimations)
                    if (anim.Enabled)
                        ExecuteAnimation(anim.Effect);
            }
        }

        int timer = 40;
        /// <summary>
        /// The intervall in ms after each animation cycle is calculated (default: 40)
        /// </summary>
        public int AnimationTimer
        {
            get => timer;
            set
            {
                if (value > 0) timer = value;
            }
        }


        bool enableTimer = false;
        /// <summary>
        /// Start the animation runtime
        /// </summary>
        public void StartTimer()
        {
            enableTimer = true;
            var thread = new Thread(timerLoop);
            thread.Name = "Animation Timer";
            thread.Start();
        }

        /// <summary>
        /// Stops the animation runtime
        /// </summary>
        public void StopTimer()
        {
            enableTimer = false;
        }

        /// <summary>
        /// calculation loop
        /// </summary>
        void timerLoop()
        {
            while (enableTimer)
            {
                for (var i = 0; i < Running.Count; ++i)
                    PerformAnimation(Running[i]);
                PerformStatus();
                Thread.Sleep(timer);
            }
        }
    }
}
