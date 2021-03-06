﻿using System.Threading;

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

namespace Sys.Workflow.Engine.Impl.Asyncexecutor.Multitenants
{
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Cfg.Multitenants;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;

    /// <summary>
    /// Factory that produces a <seealso cref="Runnable"/> that executes a <seealso cref="IJobEntity"/>.
    /// Can be used to create special implementations for specific tenants.
    /// 
    /// 
    /// </summary>
    public class TenantAwareExecuteAsyncRunnableFactory : IExecuteAsyncRunnableFactory
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ITenantInfoHolder tenantInfoHolder;

        /// <summary>
        /// 
        /// </summary>
        protected internal string tenantId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantInfoHolder"></param>
        /// <param name="tenantId"></param>
        public TenantAwareExecuteAsyncRunnableFactory(ITenantInfoHolder tenantInfoHolder, string tenantId)
        {
            this.tenantInfoHolder = tenantInfoHolder;
            this.tenantId = tenantId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="processEngineConfiguration"></param>
        /// <returns></returns>
        public virtual ThreadStart CreateExecuteAsyncRunnable(IJob job, ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            throw new System.NotImplementedException();
            //return new TenantAwareExecuteAsyncRunnable(job, processEngineConfiguration, tenantInfoHolder, tenantId);
        }
    }
}