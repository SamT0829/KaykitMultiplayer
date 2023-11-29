using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MissionGroup
{
    private List<MissionBase> missionList = new List<MissionBase>();

    public int TotalMissionCount
    {
        get
        {
            int count = 0;
            missionList.ForEach((mission) => count += mission.SubMissionCount);
            return count;
        }
    }

    public int CompleteCount
    {
        get
        {
            int completeCount = 0;
            missionList.ForEach((mission) => completeCount += mission.CompleteCount);
            return completeCount;
        }
    }


    public bool isComplete = false;

    public void AddMission(MissionBase mission)
    {
        missionList.Add(mission);
        mission.MyMissionGroup = this;
    }

    public async Task MissionGroupPrepare()
    {
        for (int i = 0; i < this.missionList.Count; i++)
        {
            Debug.Log("MissionGroupPrepare :" + this.missionList[i].Name);
            await this.missionList[i].MissionPrepare();
        }
    }

    public async Task MissionGroupWork()
    {
        await MissionGroupPrepare();
        await MissionWork();
    }

    public async Task MissionWork()
    {
        foreach (var mission in missionList)
        {
            Debug.Log("MissionWork :" + mission.Name);
            await mission.MissionWork();
        }
    }

    public void MissionGroupFinish()
    {
        Debug.Log("mission Finish");

        missionList.Clear();
    }
}
