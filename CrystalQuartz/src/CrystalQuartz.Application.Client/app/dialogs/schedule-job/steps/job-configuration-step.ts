import { ConfigurationStep, ConfigurationStepData } from './configuration-step';
import { SelectOption } from '../../common/select-option';

import __map from 'lodash/map';
import __find from 'lodash/find';
import { SchedulerExplorer } from '../../../scheduler-explorer';
import { TypeInfo } from '../../../api';
import { Validators } from '../../common/validation/validators';
import { MAP } from '../../../global/map';
import { JobGroupType } from './group-configuration-step';
import { ValidatorsFactory } from '../../common/validation/validators-factory';
import { Owner } from '../../../global/owner';

export class JobType {
    static Existing = 'existing';
    static New = 'new';
}

export class JobConfigurationStep extends Owner implements ConfigurationStep {
    code = 'job';
    navigationLabel = 'Configure Job';

    jobType = new js.ObservableValue<string>();
    jobTypeOptions = new js.ObservableList<SelectOption>();
    existingJobs = new js.ObservableList<SelectOption>();
    selectedJob = new js.ObservableValue<string>();
    newJobName = new js.ObservableValue<string>();
    //job class start ↓
    newJobClass = new js.ObservableValue<string>();

    newClassName = new js.ObservableValue<string>();
    newNameSpace = new js.ObservableValue<string>();
    newDllName = new js.ObservableValue<string>();
    //job calss end ↑
    allowedJobTypes = new js.ObservableList<SelectOption>();

    validators = new Validators();

    constructor(private schedulerExplorer: SchedulerExplorer, allowedJobTypes: TypeInfo[]) {
        super();

        //const values = __map(
        //    allowedJobTypes,
        //    type => {
        //        const formattedType = type.Namespace + '.' + type.Name + ', ' + type.Assembly;
        //        //拼接 下拉框中的 job class 选项
        //        return ({ value: formattedType, title: formattedType });
        //    });

        ////添加一个默认值,并将 后续的选项 拼接
        //this.allowedJobTypes.setValue([
        //    { value: '', title: '- 选择一个Job Class -' },
        //    ...values]);

        //注册 验证器
        this.validators.register(
            {
                source: this.selectedJob, //源数据
                condition: this.own(MAP(this.jobType, x => x === JobType.Existing)) //条件
            },
            ValidatorsFactory.required('请选择一个job'));

        this.validators.register(
            {
                source: this.newDllName,
                condition: this.own(MAP(this.jobType, x => x === JobType.New))
            },
            ValidatorsFactory.required('请填写一个 程序集名称'));
        this.validators.register(
            {
                source: this.newNameSpace,
                condition: this.own(MAP(this.jobType, x => x === JobType.New))
            },
            ValidatorsFactory.required('请填写一个 命名空间'));
        this.validators.register(
            {
                source: this.newClassName,
                condition: this.own(MAP(this.jobType, x => x === JobType.New))
            },
            ValidatorsFactory.required('请填写一个 Class 名称'));

        this.own(this.validators);
    }

    onEnter(data: ConfigurationStepData): ConfigurationStepData {
        const
            selectedJobGroupName = data.groupName || 'Default',
            jobGroup = __find(this.schedulerExplorer.listGroups(), x => x.Name === selectedJobGroupName),
            options: SelectOption[] = [
                { title: '定义新job', value: JobType.New }
            ];

        const canUseExistingJob = jobGroup && jobGroup.Jobs && jobGroup.Jobs.length > 0;
        if (canUseExistingJob) {
            options.push({ title: '使用存在的job', value: JobType.Existing });

            const existingJobsOption = [
                { value: '', title: '- 选择一个job -' },
                ...__map(jobGroup.Jobs, j => ({ value: j.Name, title: j.Name }))];

            this.existingJobs.setValue(existingJobsOption);
            this.selectedJob.setValue(this.selectedJob.getValue());
        }

        const
            currentJobType = this.jobType.getValue(),
            existingJobType = currentJobType === JobType.Existing,
            shouldResetSelectedJob = existingJobType &&
                ((!jobGroup) ||
                    !__find(jobGroup.Jobs, j => j.Name === this.selectedJob.getValue()));

        if (shouldResetSelectedJob) {
            this.selectedJob.setValue(null);
        }

        this.jobTypeOptions.setValue(options);

        const shouldResetJobType = currentJobType == null || (!canUseExistingJob && existingJobType);

        if (shouldResetJobType) {
            this.jobType.setValue(JobType.New);
        } else {
            this.jobType.setValue(currentJobType);
        }

        return data;
    }

    onLeave(data: ConfigurationStepData): ConfigurationStepData {
        return {
            groupName: data.groupName,
            jobName: this.getJobName(),
            jobClass: this.getJobClass()
        };
    }

    /**
     * 获取jobName
     */
    getJobName(): string {
        const jobType = this.jobType.getValue();

        switch (jobType) {
            case JobType.New: {
                return this.newJobName.getValue();
            }
            case JobType.Existing: {
                return this.selectedJob.getValue();
            }
            default: {
                throw new Error('未知的 job type ' + jobType);
            }
        }
    }

    /**
     * 获取 jobClass
     */
    getJobClass(): string | null {
        const jobType = this.jobType.getValue();

        switch (jobType) {
            case JobType.New: {

                var newJobClass = this.newNameSpace.getValue() + "." + this.newClassName.getValue() + "," + this.newDllName.getValue();
                
                return newJobClass;
            }
            case JobType.Existing: {
                return null;
            }
            default: {
                throw new Error('未知 job type ' + jobType);
            }
        }
    }

    releaseState() {
        this.dispose();
    }
}