using System;
namespace DataCapture.Workflow.Yeti
{
    public enum WorkItemState
    {
        Available = 1
        , Locked = 2
        , InProgress = 3
        , Suspended = 5
        , Terminated = 9
    }
}
