using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class MissionBase
{
    public int CompleteCount { get; set; }
    public int SubMissionCount { get; set; }
    public bool IsComplete { get; set; }
    public bool IsFinish { get; set; }
    public bool IsFail { get; set; }
    public string Name { get; set; }

    public MissionGroup MyMissionGroup;
    public Func<Task> OnProcess;
    public Func<Task> OnProgress;
    public Func<Task> OnComplete;
    public Func<Task> OnFinish;
    public Func<Task> OnFail;

    public MissionBase()
    {
        this.Name = this.GetType().Name;
        MissionReset();
    }

    public virtual Task MissionPrepare()
    {
        SubMissionCount = 6;
        CompleteCount = 0;
        return Task.CompletedTask;
    }

    public virtual async Task MissionWork()
    {
        await MissionProcess();

        while (!this.IsComplete)
        {
            await Task.Delay(TimeSpan.FromSeconds(0.05));
        }

        await Task.Delay(TimeSpan.FromSeconds(0.05));

        await MissionFinish();
    }

    protected abstract Task MissionProcess();

    protected virtual Task MissionComplete()
    {
        CompleteCount = SubMissionCount;
        IsComplete = true;

        if (OnComplete != null)
            return OnComplete.Invoke();

        return Task.CompletedTask;
    }

    protected virtual Task MissionFail()
    {
        IsFail = true;

        if (OnFail != null)
            return OnFail.Invoke();

        return Task.CompletedTask;
    }

    protected virtual async Task MissionFinish()
    {
        IsFinish = true;

        if (OnFinish != null)
            await OnFinish.Invoke();

        Debug.Log(Name + " MissionFinish");
        await Task.CompletedTask;
    }

    protected virtual void MissionReset()
    {
        CompleteCount = 0;
        IsComplete = false;
        IsFinish = false;
        IsFail = false;
    }
}