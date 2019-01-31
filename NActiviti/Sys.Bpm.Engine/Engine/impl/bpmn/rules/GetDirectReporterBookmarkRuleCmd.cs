///////////////////////////////////////////////////////////
//  GetDirectReporterBookmarkRuleCmd.cs
//  Implementation of the Class GetDirectReporterBookmarkRuleCmd
//  Generated by Enterprise Architect
//  Created on:      30-1月-2019 8:32:00
//  Original author: 张楠
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using org.activiti.engine.impl.interceptor;

namespace Sys.Workflow.Engine.Bpmn.Rules
{
    /// <summary>
    /// 根据人员id查找直接汇报对象
    /// </summary>
    [GetBookmarkDescriptor("GetDirectReporter")]
    public class GetDirectReporterBookmarkRuleCmd : IGetBookmarkRule
    {

        public GetDirectReporterBookmarkRuleCmd()
        {

        }

        public QueryBookmark Condition { get; set; }

        /// 
        /// <param name="userId"></param>
        private IList<IUserInfo> 获取发起者直接汇报对象(string userId)
        {

            return null;
        }

        public IList<IUserInfo> execute(ICommandContext commandContext)
        {
            throw new NotImplementedException();
        }
    }
}