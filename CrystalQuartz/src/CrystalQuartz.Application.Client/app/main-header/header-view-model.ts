import { SchedulerData } from '../api';
import { CommandService  } from '../services';
import { ICommand  } from '../commands/contracts';
import { StartSchedulerCommand, StopSchedulerCommand, PauseSchedulerCommand, ResumeSchedulerCommand, StandbySchedulerCommand } from '../commands/scheduler-commands';
import { ApplicationModel } from '../application-model';
import CommandProgressViewModel from '../command-progress/command-progress-view-model';
import Timeline from '../timeline/timeline';

import { IDialogManager } from '../dialogs/dialog-manager';
import SchedulerDetails from '../dialogs/scheduler-details/scheduler-details-view-model';

import Action from '../global/actions/action';
import CommandAction from '../command-action';
import {ScheduleJobViewModel} from '../dialogs/schedule-job/schedule-job-view-model';
import {JobGroup} from '../api';
import {SchedulerExplorer} from '../scheduler-explorer';
import {SHOW_SCHEDULE_JOB_DIALOG} from '../dialogs/show-schedule-job-dialog';

export default class MainHeaderViewModel {
    name = new js.ObservableValue<string>();
    instanceId = js.observableValue<string>();

    status = new js.ObservableValue<string>();
    
    startAction = new CommandAction(this.application, this.commandService, '开始', () => new StartSchedulerCommand());
    pauseAllAction = new CommandAction(this.application, this.commandService, '暂停所有', () => new PauseSchedulerCommand());
    resumeAllAction = new CommandAction(this.application, this.commandService, '重启所有', () => new ResumeSchedulerCommand());
    standbyAction = new CommandAction(this.application, this.commandService, '暂停调度器', () => new StandbySchedulerCommand());
    shutdownAction = new CommandAction(this.application, this.commandService, '关机', () => new StopSchedulerCommand(), 'Are you sure you want to shutdown scheduler?');

    scheduleJobAction = new Action(
        '+',
        () => { this.scheduleJob(); });

    commandProgress = new CommandProgressViewModel(this.commandService);

    constructor(
        public timeline: Timeline,
        private commandService: CommandService,
        private application: ApplicationModel,
        private dialogManager: IDialogManager) { }

    updateFrom(data: SchedulerData) {
        this.name.setValue(data.Name);
        this.instanceId.setValue(data.InstanceId);
        this.status.setValue(data.Status);

        this.startAction.enabled = data.Status === 'ready';
        this.shutdownAction.enabled = (data.Status !== 'shutdown');
        this.standbyAction.enabled = data.Status === 'started';
        this.pauseAllAction.enabled = data.Status === 'started';
        this.resumeAllAction.enabled = data.Status === 'started';
        this.scheduleJobAction.enabled = data.Status !== 'shutdown';
    }

    /**
     * 显示 调度器 详情
     */
    showSchedulerDetails() {
        this.dialogManager.showModal(new SchedulerDetails(this.commandService), result => {});
    }

    private scheduleJob() {
        SHOW_SCHEDULE_JOB_DIALOG(
            this.dialogManager,
            this.application,
            this.commandService);
    }
}