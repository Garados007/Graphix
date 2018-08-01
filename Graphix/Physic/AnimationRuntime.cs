using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Graphix.Physic
{
    public class AnimationRuntime
    {
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

        class StatusPushInfo
        {
            public Status Status;
            public int Delay;
            public int? Started;
        }

        public delegate void StatusChangedHandler(Status oldStatus, Status newStatus);

        List<AnimationGroup> Animations = new List<AnimationGroup>();
        List<RunningAnimation> Running = new List<RunningAnimation>();

        public ISoundPlayer SoundPlayer { get; set; }

        Status currentStatus = null;
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

        public event StatusChangedHandler StatusChanged;

        Queue<StatusPushInfo> StatusQueue = new Queue<StatusPushInfo>();

        public void PushStatus(Status status, int delay)
        {
            StatusQueue.Enqueue(new StatusPushInfo()
            {
                Status = status,
                Delay = delay,
                Started = null
            });
        }

        public void FlushStatusQueue()
        {
            while (StatusQueue.Count > 0)
            {
                var info = StatusQueue.Dequeue();
                CurrentStatus = info.Status;
            }
        }

        public void ClearStatusQueue()
        {
            StatusQueue.Clear();
        }

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
        public int AnimationTimer
        {
            get => timer;
            set
            {
                if (value > 0) timer = value;
            }
        }


        bool enableTimer = false;
        public void StartTimer()
        {
            enableTimer = true;
            var thread = new Thread(timerLoop);
            thread.Name = "Animation Timer";
            thread.Start();
        }

        public void StopTimer()
        {
            enableTimer = false;
        }

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
