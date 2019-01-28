﻿using System;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.db;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.task;

    /// 
    /// 
    /// 
    /// 
    [Serializable]
    public class TaskEntityImpl : VariableScopeImpl, ITaskEntity, IBulkDeleteable
    {

        public const string DELETE_REASON_COMPLETED = "completed";
        public const string DELETE_REASON_DELETED = "deleted";

        private const long serialVersionUID = 1L;

        protected internal string owner;
        protected internal int assigneeUpdatedCount; // needed for v5 compatibility
        protected internal string originalAssignee; // needed for v5 compatibility
        protected internal string assignee;
        protected internal DelegationState? delegationState;

        protected internal string parentTaskId;

        protected internal string name;
        protected internal string localizedName;
        protected internal string description;
        protected internal string localizedDescription;
        protected internal int? priority = Task_Fields.DEFAULT_PRIORITY;
        protected internal DateTime? createTime; // The time when the task has been created
        protected internal DateTime? dueDate;
        protected internal int suspensionState = SuspensionState_Fields.ACTIVE.StateCode;
        protected internal string category;

        protected internal bool isIdentityLinksInitialized;
        protected internal IList<IIdentityLinkEntity> taskIdentityLinkEntities = new List<IIdentityLinkEntity>();

        protected internal string executionId;
        protected internal IExecutionEntity execution;

        protected internal string processInstanceId;
        protected internal IExecutionEntity processInstance;

        protected internal string processDefinitionId;

        protected internal string taskDefinitionKey;
        protected internal string formKey;

        protected internal new bool isDeleted;
        protected internal bool isCanceled;
        protected internal string eventName;
        protected internal ActivitiListener currentActivitiListener;

        protected internal string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;

        protected internal IList<IVariableInstanceEntity> queryVariables;

        protected internal bool forcedUpdate;

        protected internal DateTime? claimTime;

        public TaskEntityImpl()
        {

        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState();
                persistentState["assignee"] = this.assignee;
                persistentState["owner"] = this.owner;
                persistentState["name"] = this.name;
                persistentState["priority"] = this.priority;
                if (!string.IsNullOrWhiteSpace(executionId))
                {
                    persistentState["executionId"] = this.executionId;
                }
                if (!string.IsNullOrWhiteSpace(processDefinitionId))
                {
                    persistentState["processDefinitionId"] = this.processDefinitionId;
                }
                if (createTime.HasValue)
                {
                    persistentState["createTime"] = this.createTime;
                }
                if (!string.IsNullOrWhiteSpace(description))
                {
                    persistentState["description"] = this.description;
                }
                if (dueDate.HasValue)
                {
                    persistentState["dueDate"] = this.dueDate;
                }
                if (!ReferenceEquals(parentTaskId, null))
                {
                    persistentState["parentTaskId"] = this.parentTaskId;
                }

                persistentState["suspensionState"] = this.suspensionState;

                if (forcedUpdate)
                {
                    persistentState["forcedUpdate"] = true;
                }

                if (claimTime.HasValue)
                {
                    persistentState["claimTime"] = this.claimTime;
                }

                return persistentState;
            }
        }

        public override int RevisionNext
        {
            get
            {
                return revision + 1;
            }
        }

        public virtual void forceUpdate()
        {
            this.forcedUpdate = true;
        }

        // variables //////////////////////////////////////////////////////////////////

        protected internal override VariableScopeImpl ParentVariableScope
        {
            get
            {
                if (Execution != null)
                {
                    return (ExecutionEntityImpl)execution;
                }
                return null;
            }
        }

        protected internal override void initializeVariableInstanceBackPointer(IVariableInstanceEntity variableInstance)
        {
            variableInstance.TaskId = id;
            variableInstance.ExecutionId = executionId;
            variableInstance.ProcessInstanceId = processInstanceId;
        }

        protected internal override IList<IVariableInstanceEntity> loadVariableInstances()
        {
            return Context.CommandContext.VariableInstanceEntityManager.findVariableInstancesByTaskId(id);
        }

        protected internal override IVariableInstanceEntity createVariableInstance(string variableName, object value, IExecutionEntity sourceActivityExecution)
        {
            IVariableInstanceEntity result = base.createVariableInstance(variableName, value, sourceActivityExecution);

            // Dispatch event, if needed
            if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createVariableEvent(ActivitiEventType.VARIABLE_CREATED, variableName, value, result.Type, result.TaskId, result.ExecutionId, ProcessInstanceId, ProcessDefinitionId));
            }
            return result;
        }

        protected internal override void updateVariableInstance(IVariableInstanceEntity variableInstance, object value, IExecutionEntity sourceActivityExecution)
        {
            base.updateVariableInstance(variableInstance, value, sourceActivityExecution);

            // Dispatch event, if needed
            if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createVariableEvent(ActivitiEventType.VARIABLE_UPDATED, variableInstance.Name, value, variableInstance.Type, variableInstance.TaskId, variableInstance.ExecutionId, ProcessInstanceId, ProcessDefinitionId));
            }
        }

        // execution //////////////////////////////////////////////////////////////////

        public virtual IExecutionEntity Execution
        {
            get
            {
                var ctx = Context.CommandContext;
                if (execution == null && !string.IsNullOrWhiteSpace(executionId) && ctx != null)
                {
                    this.execution = ctx.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", executionId));
                }
                return execution;
            }
            set
            {
                this.execution = value;
            }
        }

        // task assignment ////////////////////////////////////////////////////////////

        public virtual void addCandidateUser(string userId)
        {
            Context.CommandContext.IdentityLinkEntityManager.addCandidateUser(this, userId);
        }

        public virtual void addCandidateUsers(ICollection<string> candidateUsers)
        {
            Context.CommandContext.IdentityLinkEntityManager.addCandidateUsers(this, candidateUsers);
        }

        public virtual void addCandidateGroup(string groupId)
        {
            Context.CommandContext.IdentityLinkEntityManager.addCandidateGroup(this, groupId);
        }

        public virtual void addCandidateGroups(ICollection<string> candidateGroups)
        {
            Context.CommandContext.IdentityLinkEntityManager.addCandidateGroups(this, candidateGroups);
        }

        public virtual void addUserIdentityLink(string userId, string identityLinkType)
        {
            Context.CommandContext.IdentityLinkEntityManager.addUserIdentityLink(this, userId, identityLinkType);
        }

        public virtual void addGroupIdentityLink(string groupId, string identityLinkType)
        {
            Context.CommandContext.IdentityLinkEntityManager.addGroupIdentityLink(this, groupId, identityLinkType);
        }

        public virtual ISet<IIdentityLink> Candidates
        {
            get
            {
                ISet<IIdentityLink> potentialOwners = new HashSet<IIdentityLink>();
                foreach (IIdentityLinkEntity identityLinkEntity in IdentityLinks)
                {
                    if (IdentityLinkType.CANDIDATE.Equals(identityLinkEntity.Type))
                    {
                        potentialOwners.Add(identityLinkEntity);
                    }
                }
                return potentialOwners;
            }
        }

        public virtual void deleteCandidateGroup(string groupId)
        {
            deleteGroupIdentityLink(groupId, IdentityLinkType.CANDIDATE);
        }

        public virtual void deleteCandidateUser(string userId)
        {
            deleteUserIdentityLink(userId, IdentityLinkType.CANDIDATE);
        }

        public virtual void deleteGroupIdentityLink(string groupId, string identityLinkType)
        {
            if (!ReferenceEquals(groupId, null))
            {
                Context.CommandContext.IdentityLinkEntityManager.deleteIdentityLink(this, null, groupId, identityLinkType);
            }
        }

        public virtual void deleteUserIdentityLink(string userId, string identityLinkType)
        {
            if (!ReferenceEquals(userId, null))
            {
                Context.CommandContext.IdentityLinkEntityManager.deleteIdentityLink(this, userId, null, identityLinkType);
            }
        }

        public virtual IList<IIdentityLinkEntity> IdentityLinks
        {
            get
            {
                var ctx = Context.CommandContext;
                if (!isIdentityLinksInitialized && ctx != null)
                {
                    taskIdentityLinkEntities = ctx.IdentityLinkEntityManager.findIdentityLinksByTaskId(id);
                    isIdentityLinksInitialized = true;
                }

                return taskIdentityLinkEntities;
            }
        }

        public virtual IDictionary<string, object> ExecutionVariables
        {
            set
            {
                if (Execution != null)
                {
                    execution.Variables = value;
                }
            }
        }

        public virtual string Name
        {
            set
            {
                this.name = value;
            }
            get
            {
                if (!ReferenceEquals(localizedName, null) && localizedName.Length > 0)
                {
                    return localizedName;
                }
                else
                {
                    return name;
                }
            }
        }

        public virtual string Description
        {
            set
            {
                this.description = value;
            }
            get
            {
                if (!ReferenceEquals(localizedDescription, null) && localizedDescription.Length > 0)
                {
                    return localizedDescription;
                }
                else
                {
                    return description;
                }
            }
        }

        public virtual string Assignee
        {
            set
            {
                this.originalAssignee = this.assignee;
                this.assignee = value;
                assigneeUpdatedCount++;
            }
            get
            {
                return assignee;
            }
        }

        public virtual string Owner
        {
            set
            {
                this.owner = value;
            }
            get
            {
                return owner;
            }
        }

        public virtual DateTime? DueDate
        {
            set
            {
                this.dueDate = value;
            }
            get
            {
                return dueDate;
            }
        }

        public virtual int? Priority
        {
            set
            {
                this.priority = value;
            }
            get
            {
                return priority;
            }
        }

        public virtual string Category
        {
            set
            {
                this.category = value;
            }
            get
            {
                return category;
            }
        }

        public virtual string ParentTaskId
        {
            set
            {
                this.parentTaskId = value;
            }
            get
            {
                return parentTaskId;
            }
        }

        public virtual string FormKey
        {
            get
            {
                return formKey;
            }
            set
            {
                this.formKey = value;
            }
        }


        // Override from VariableScopeImpl

        protected internal override bool ActivityIdUsedForDetails
        {
            get
            {
                return false;
            }
        }

        // Overridden to avoid fetching *all* variables (as is the case in the super // call)
        protected internal override IVariableInstanceEntity getSpecificVariable(string variableName)
        {
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext == null)
            {
                throw new ActivitiException("lazy loading outside command context");
            }
            IVariableInstanceEntity variableInstance = commandContext.VariableInstanceEntityManager.findVariableInstanceByTaskAndName(id, variableName);

            return variableInstance;
        }

        protected internal override IList<IVariableInstanceEntity> getSpecificVariables(ICollection<string> variableNames)
        {
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext == null)
            {
                throw new ActivitiException("lazy loading outside command context");
            }
            return commandContext.VariableInstanceEntityManager.findVariableInstancesByTaskAndNames(id, variableNames);
        }

        // regular getters and setters ////////////////////////////////////////////////////////

        public override int Revision
        {
            get
            {
                return revision;
            }
            set
            {
                this.revision = value;
            }
        }



        public virtual string LocalizedName
        {
            get
            {
                return localizedName;
            }
            set
            {
                this.localizedName = value;
            }
        }



        public virtual string LocalizedDescription
        {
            get
            {
                return localizedDescription;
            }
            set
            {
                this.localizedDescription = value;
            }
        }




        public virtual DateTime? CreateTime
        {
            get
            {
                return createTime;
            }
            set
            {
                this.createTime = value;
            }
        }


        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
            set
            {
                this.executionId = value;
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
            set
            {
                this.processInstanceId = value;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId;
            }
            set
            {
                this.processDefinitionId = value;
            }
        }



        public virtual string OriginalAssignee
        {
            get
            {
                // Don't ask. A stupid hack for v5 compatibility
                if (assigneeUpdatedCount > 1)
                {
                    return originalAssignee;
                }
                else
                {
                    return assignee;
                }
            }
        }

        public virtual string TaskDefinitionKey
        {
            get
            {
                return taskDefinitionKey;
            }
            set
            {
                this.taskDefinitionKey = value;
            }
        }


        public virtual string EventName
        {
            get
            {
                return eventName;
            }
            set
            {
                this.eventName = value;
            }
        }


        public virtual ActivitiListener CurrentActivitiListener
        {
            get
            {
                return currentActivitiListener;
            }
            set
            {
                this.currentActivitiListener = value;
            }
        }



        public virtual IExecutionEntity ProcessInstance
        {
            get
            {
                var ctx = Context.CommandContext;
                if (processInstance == null && processInstanceId != null && ctx != null)
                {
                    processInstance = ctx.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));
                }
                return processInstance;
            }
            set
            {
                this.processInstance = value;
            }
        }





        public virtual DelegationState? DelegationState
        {
            get
            {
                return delegationState;
            }
            set
            {
                this.delegationState = value;
            }
        }


        public virtual string DelegationStateString
        {
            get
            {
                //Needed for Activiti 5 compatibility, not exposed in terface
                return delegationState.HasValue ? DelegationState.ToString() : null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    delegationState = null;
                }

                if (Enum.TryParse<DelegationState>(value, out var state))
                {
                    delegationState = null;
                }
                else
                {
                    delegationState = state;
                }
            }
        }


        public override bool Deleted
        {
            get
            {
                return isDeleted;
            }
            set
            {
                this.isDeleted = value;
            }
        }


        public virtual bool Canceled
        {
            get
            {
                return isCanceled;
            }
            set
            {
                this.isCanceled = value;
            }
        }



        public override IDictionary<string, IVariableInstanceEntity> VariableInstanceEntities
        {
            get
            {
                ensureVariableInstancesInitialized();
                return variableInstances;
            }
        }

        public virtual int SuspensionState
        {
            get
            {
                return suspensionState;
            }
            set
            {
                this.suspensionState = value;
            }
        }



        public virtual bool Suspended
        {
            get
            {
                return suspensionState == SuspensionState_Fields.SUSPENDED.StateCode;
            }
            set
            {

            }
        }

        public virtual IDictionary<string, object> TaskLocalVariables
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                if (queryVariables != null)
                {
                    foreach (IVariableInstanceEntity variableInstance in queryVariables)
                    {
                        if (!ReferenceEquals(variableInstance.Id, null) && !ReferenceEquals(variableInstance.TaskId, null))
                        {
                            variables[variableInstance.Name] = variableInstance.Value;
                        }
                    }
                }
                return variables;
            }
        }

        public virtual IDictionary<string, object> ProcessVariables
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                if (queryVariables != null)
                {
                    foreach (IVariableInstanceEntity variableInstance in queryVariables)
                    {
                        if (!ReferenceEquals(variableInstance.Id, null) && ReferenceEquals(variableInstance.TaskId, null))
                        {
                            variables[variableInstance.Name] = variableInstance.Value;
                        }
                    }
                }
                return variables;
            }
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set
            {
                this.tenantId = value;
            }
        }


        public virtual IList<IVariableInstanceEntity> QueryVariables
        {
            get
            {
                if (queryVariables == null && Context.CommandContext != null)
                {
                    queryVariables = new VariableInitializingList();
                }
                return queryVariables;
            }
            set
            {
                this.queryVariables = value;
            }
        }


        public virtual DateTime? ClaimTime
        {
            get
            {
                return claimTime;
            }
            set
            {
                this.claimTime = value;
            }
        }


        public override string ToString()
        {
            return "Task[id=" + id + ", name=" + name + "]";
        }
    }

}