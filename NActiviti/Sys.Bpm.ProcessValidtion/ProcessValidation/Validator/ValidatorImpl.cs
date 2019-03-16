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
namespace org.activiti.validation.validator
{

    using org.activiti.bpmn.model;

    /// 
    public abstract class ValidatorImpl : IValidator
    {
        public abstract void validate(org.activiti.bpmn.model.BpmnModel bpmnModel, IList<ValidationError> errors);

        public virtual void addError(IList<ValidationError> validationErrors, ValidationError error)
        {
            validationErrors.Add(error);
        }

        protected internal virtual void addError(IList<ValidationError> validationErrors, string problem, string description)
        {
            addError(validationErrors, problem, null, null, description, false);
        }

        protected internal virtual void addError(IList<ValidationError> validationErrors, string problem, BaseElement baseElement, string description)
        {
            addError(validationErrors, problem, null, baseElement, description);
        }

        protected internal virtual void addError(IList<ValidationError> validationErrors, string problem, Process process, BaseElement baseElement, string description)
        {
            addError(validationErrors, problem, process, baseElement, description, false);
        }

        protected internal virtual void addWarning(IList<ValidationError> validationErrors, string problem, Process process, BaseElement baseElement, string description)
        {
            addError(validationErrors, problem, process, baseElement, description, true);
        }

        protected internal virtual void addError(IList<ValidationError> validationErrors, string problem, Process process, BaseElement baseElement, string description, bool isWarning)
        {
            ValidationError error = new ValidationError();
            error.Warning = isWarning;

            if (process != null)
            {
                error.ProcessDefinitionId = process.Id;
                error.ProcessDefinitionName = process.Name;
            }

            if (baseElement != null)
            {
                error.XmlLineNumber = baseElement.XmlRowNumber;
                error.XmlColumnNumber = baseElement.XmlColumnNumber;
            }
            error.Problem = problem;
            error.DefaultDescription = description;

            if (baseElement is FlowElement)
            {
                FlowElement flowElement = (FlowElement)baseElement;
                error.ActivityId = flowElement.Id;
                error.ActivityName = flowElement.Name;
            }

            addError(validationErrors, error);
        }

        protected internal virtual void addError(IList<ValidationError> validationErrors, string problem, Process process, string id, string description)
        {
            ValidationError error = new ValidationError();

            if (process != null)
            {
                error.ProcessDefinitionId = process.Id;
                error.ProcessDefinitionName = process.Name;
            }

            error.Problem = problem;
            error.DefaultDescription = description;
            error.ActivityId = id;

            addError(validationErrors, error);
        }

    }

}