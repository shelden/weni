using System;
using System.Text;
using System.Collections.Generic;
using DataCapture.Workflow.Db;

namespace DataCapture.Workflow
{
    public class WorkItemInfo : Dictionary<String, String>
    {
        #region members
        #endregion

        #region properties
        public int Id { get; private set; }
        public String Name { get; private set; }
        public String MapName { get; private set; }
        public int MapVersion { get; private set; }
        public int State { get; private set; } // XXX move this enum up to this package
        public int Priority { get; private set; }
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
            this.State = (int)item.ItemState;
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
            sb.Append(", created=");
            sb.Append(this.Created);
            return sb.ToString();
        }
        #endregion
    }
}
