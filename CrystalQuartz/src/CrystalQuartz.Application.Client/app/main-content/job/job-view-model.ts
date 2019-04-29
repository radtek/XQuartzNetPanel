import { Job, Trigger } from '../../api';
import { PauseJobCommand, ResumeJobCommand, DeleteJobCommand, ExecuteNowCommand } from '../../commands/job-commands';
import { CommandService } from '../../services';
import { ApplicationModel } from '../../application-model';
import { ManagableActivityViewModel } from '../activity-view-model';
import ActivitiesSynschronizer from '../activities-synschronizer';
import { TriggerViewModel } from '../trigger/trigger-view-model';
import Timeline from '../../timeline/timeline';

import { IDialogManager } from '../../dialogs/dialog-manager';
import JobDetailsViewModel from '../../dialogs/job-details/job-details-view-model';

import { ISchedulerStateService } from '../../scheduler-state-service';

import CommandAction from '../../command-action';
import Action from '../../global/actions/action';
import {SHOW_SCHEDULE_JOB_DIALOG} from '../../dialogs/show-schedule-job-dialog';

export class JobViewModel extends ManagableActivityViewModel<Job> {
    triggers = js.observableList<TriggerViewModel>();

    executeNowAction = new CommandAction(this.applicationModel, this.commandService, '立即执行', () => new ExecuteNowCommand(this.group, this.name));
    addTriggerAction = new Action('添加触发器', () => this.addTrigger());

    private triggersSynchronizer: ActivitiesSynschronizer<Trigger, TriggerViewModel> = new ActivitiesSynschronizer<Trigger, TriggerViewModel>(
        (trigger: Trigger, triggerViewModel: TriggerViewModel) => trigger.Name === triggerViewModel.name,
        (trigger: Trigger) => new TriggerViewModel(trigger, this.commandService, this.applicationModel, this.timeline, this.dialogManager, this.schedulerStateService),
        this.triggers);

    constructor(
        private job: Job,
        private group: string,
        commandService: CommandService,
        applicationModel: ApplicationModel,
        private timeline: Timeline,
        private dialogManager: IDialogManager,
        private schedulerStateService: ISchedulerStateService) {

        super(job, commandService, applicationModel);
    }

    /**
     * 加载job详情
     */
    loadJobDetails() {
        this.dialogManager.showModal(new JobDetailsViewModel(this.job, this.commandService), result => {});
    }
    
    updateFrom(job: Job) {
        super.updateFrom(job);

        this.triggersSynchronizer.sync(job.Triggers);
    }
    /**
     * 获取删除提示文字
     */
    getDeleteConfirmationsText(): string {
        return '你想要删除这个job吗?';
    }
    /**
     * 获取 暂停所有事件
     */
    getPauseAction() {
        return {
            title: '暂停所有触发器',
            command: () => new PauseJobCommand(this.group, this.name)
        };
    }
    /**
     * 获取 重启所有事件
     */
    getResumeAction() {
        return {
            title: '重启所有触发器',
            command: () => new ResumeJobCommand(this.group, this.name)
        };
    }
    /**
     * 获取 删除 事件
     */
    getDeleteAction() {
        return {
            title: '删除 job',
            command: () => new DeleteJobCommand(this.group, this.name)
        };
    }

    /**
     * 添加触发器
     */
    private addTrigger() {
        SHOW_SCHEDULE_JOB_DIALOG(
            this.dialogManager,
            this.applicationModel,
            this.commandService,
            {
                predefinedGroup: this.job.GroupName,
                predefinedJob: this.job.Name
            });
    }
}