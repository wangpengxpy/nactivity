﻿using System.Collections.Generic;

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
namespace org.activiti.validation.validator.impl
{

    using org.activiti.bpmn.model;

    /// 
    public class EventGatewayValidator : ProcessLevelValidator
    {

        protected internal override void executeValidation(BpmnModel bpmnModel, Process process, IList<ValidationError> errors)
        {
            IList<EventGateway> eventGateways = process.findFlowElementsOfType<EventGateway>();
            foreach (EventGateway eventGateway in eventGateways)
            {
                foreach (SequenceFlow sequenceFlow in eventGateway.OutgoingFlows)
                {
                    FlowElement flowElement = process.getFlowElement(sequenceFlow.TargetRef, true);
                    if (flowElement != null && !(flowElement is IntermediateCatchEvent))
                    {
                        addError(errors, Problems_Fields.EVENT_GATEWAY_ONLY_CONNECTED_TO_INTERMEDIATE_EVENTS, process, eventGateway, "Event based gateway can only be connected to elements of type intermediateCatchEvent");
                    }
                }
            }
        }

    }

}