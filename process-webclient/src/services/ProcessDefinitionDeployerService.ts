
import { IProcessDefinitionDeployerService } from './IProcessDefinitionDeployerService'
import Axios from 'axios';
import contants from 'contants';
import { IProcessDefinitionDeployer } from './IProcessDefinitionDeployer';
import { IResources } from 'model/query/IResources';

export class ProcessDefinitionDeployerService implements IProcessDefinitionDeployerService {

  private readonly controller: string = "workflow/process-deployer";

  save(deployer: IProcessDefinitionDeployer): Promise<any> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}/${this.controller}/save`, deployer).then(data => {
        res(data.data);
      });
    });
  }

  latest(query: any): Promise<IResources<any>> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}/${this.controller}/latest`, query).then(data => {
        res(data.data);
      });
    });
  }

  deploy(deployer: IProcessDefinitionDeployer): Promise<any> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}/${this.controller}`, deployer).then(data => {
        res(data.data);
      });
    });
  }

  allDeployments(query: any): Promise<IResources<any>> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}/${this.controller}/list`, query).then(data => {
        res(data.data);
      });
    });
  }

  remove(id: string): Promise<any> {
    return new Promise((res, rej) => {
      Axios.get(`${contants.serverUrl}/${this.controller}/${id}/remove`).then(data => {
        res(data.data);
      });
    });
  }

  getProcessModel(id: string): Promise<string> {
    return new Promise((res, rej) => {
      Axios.get(`${contants.serverUrl}/${this.controller}/${id}/processmodel`).then(data => {
        res(data.data);
      });
    });
  }
}
