using System;
using System.Text;
using System.Collections.Generic;
using DataCapture.Workflow.Yeti.Db;

namespace DataCapture.Workflow.Yeti
{
    public class WorkItemInfo : Dictionary<String, String>
    {
        #region members
        #endregion

        #region properties
        public int Id { get; private set; }
        public String Name { get; set; }
        public String MapName { get; private set; }
        public int MapVersion { get; private set; }
        public WorkItemState State { get; private set; }
        public int Priority { get; set; }
        public String QueueName { get; private set; }
        public String StepName { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime Entered { get; private set; }
        #endregion

        #region constructors
        public WorkItemInfo(WorkItem item, Map map, Step step, Queue queue, IList<WorkItemData> data)
        {
            this.Id = item.Id;
            this.Name = item.Name;
            this.MapName = map.Name;
            this.MapVersion = map.Version;
            this.State = (WorkItemState)item.ItemState;
            this.Priority = item.Priority;
            this.QueueName = queue.Name;
            this.StepName = step.Name;
            this.Created = item.Created;
            this.Entered = item.Entered;

            if (data == null) return;
            foreach(var kvp in data)
            {
                this.Add(kvp.VariableName, kvp.VariableValue);
            }
        }
        #endregion

        #region ToString
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.GetType().FullName);
            sb.Append(" ");
            sb.Append(this.Name);
            sb.Append(", id=");
            sb.Append(this.Id);
            sb.Append(", state=");
            sb.Append(this.State);
            sb.Append(", entered=");
            sb.Append(this.Entered);
            sb.Append(", step=");
            sb.Append(this.StepName);
            return sb.ToString();
        }
        #endregion
    }
}
