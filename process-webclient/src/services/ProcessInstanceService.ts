import { IProcessInstanceService } from "./IProcessInstanceService";
import contants from "contants";
import Axios from "axios";
import { IProcessInstanceQuery } from "model/query/IProcessInstanceQuery";
import { IStartProcessInstanceCmd } from "model/cmd/IStartProcessInstanceCmd";
import { IResources } from "model/query/IResources";
import { IProcessInstance } from "model/resource/IProcessInstance";
import { IProcessInstanceTaskQuery } from "model/query/IProcessInstanceTaskQuery";
import { ITaskModel } from "model/resource/ITaskModel";


export class ProcessInstanceServie implements IProcessInstanceService {

  private controller = "workflow/process-instances";

  getTasks(processInstanceId: string, query: IProcessInstanceTaskQuery): Promise<IResources<ITaskModel>> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}/${this.controller}/${processInstanceId}/tasks`, query).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  processInstances(query: IProcessInstanceQuery): Promise<IResources<IProcessInstance>> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}/${this.controller}`, query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }

  start(cmd: IStartProcessInstanceCmd): Promise<IProcessInstance> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}/${this.controller}/start`, cmd).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  getProcessInstanceById(processInstanceId: string): Promise<IProcessInstance> {
    return new Promise((res, rej) => {
      Axios.get(`${contants.serverUrl}/${this.controller}/${processInstanceId}`).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  getProcessDiagram(processInstanceId: string): Promise<string> {
    throw new Error("Method not implemented.");
  }

  sendSignal(cmd: any): Promise<any> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}/${this.controller}/signal`, cmd).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  suspend(processInstanceId: string): Promise<any> {
    return new Promise((res, rej) => {
      Axios.get(`${contants.serverUrl}/${this.controller}/${processInstanceId}/suspend`).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  activate(processInstanceId: string): Promise<any> {
    return new Promise((res, rej) => {
      Axios.get(`${contants.serverUrl}/${this.controller}/${processInstanceId}/activate`).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  terminate(processInstanceId: string, reason: string): Promise<any> {
    return new Promise((res, rej) => {
      Axios.get(`${contants.serverUrl}/${this.controller}/${processInstanceId}/terminate`).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }
}
