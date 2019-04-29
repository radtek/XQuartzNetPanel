import { DialogViewModel } from '../dialog-view-model';
import { CommandService } from '../../services';
import { Job, JobDetails, PropertyValue } from '../../api';
import { GetJobDetailsCommand } from '../../commands/job-commands';
import { Property, PropertyType } from '../common/property';

export default class JobDetailsViewModel extends DialogViewModel<any> {
    summary = new js.ObservableList<Property>();
    identity = new js.ObservableList<Property>();
    jobDataMap = new js.ObservableValue<PropertyValue>();

    constructor(
        private job: Job,
        private commandService: CommandService) {

        super();
    }

    loadDetails() {
        this.commandService
            .executeCommand<JobDetails>(new GetJobDetailsCommand(this.job.GroupName, this.job.Name))
            .done(details => {
                this.identity.setValue([
                    new Property('Name', this.job.Name, PropertyType.String),
                    new Property('Group', this.job.GroupName, PropertyType.String)
                ]);

                this.summary.add(
                    new Property('Job type(命名空间.类名,程序集名称)', details.JobDetails.JobType, PropertyType.Type),
                    new Property('Description(描述)', details.JobDetails.Description, PropertyType.String),
                    new Property('Concurrent execution disallowed(不允许 并发执行)', details.JobDetails.ConcurrentExecutionDisallowed, PropertyType.Boolean),
                    new Property('Persist after execution(执行后保存JobData的值_供其他触发器使用)', details.JobDetails.PersistJobDataAfterExecution, PropertyType.Boolean),
                    new Property('Requests recovery(请求恢复)', details.JobDetails.RequestsRecovery, PropertyType.Boolean),
                    new Property('Durable(是否持久,如果不持久当job的触发器数量为0的时候,会将job一起删除)', details.JobDetails.Durable, PropertyType.Boolean));

                this.jobDataMap.setValue(details.JobDataMap);
            });
    }
}