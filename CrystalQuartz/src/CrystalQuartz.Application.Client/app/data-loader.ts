import { ApplicationModel } from './application-model';
import { CommandService, ErrorInfo } from './services';
import { GetDataCommand } from './commands/global-commands';
import { SchedulerData, Job, Trigger, ActivityStatus } from './api';

import __filter from 'lodash/filter';
import __flatten from 'lodash/flatten';
import __map from 'lodash/map';
import __compact from 'lodash/compact';
import __min from 'lodash/min';

import { Timer } from "./global/timers/timer";

export class DataLoader {
    private static DEFAULT_UPDATE_INTERVAL = 30000;                 // 30sec
    private static MAX_UPDATE_INTERVAL = 300000;                    // 5min
    private static MIN_UPDATE_INTERVAL = 10000;                     // 10sec
    private static DEFAULT_UPDATE_INTERVAL_IN_PROGRESS = 20000;     // 20sec

    private _autoUpdateTimer = new Timer();

    constructor(
        private applicationModel: ApplicationModel,
        private commandService: CommandService) {
        applicationModel.onDataChanged.listen(data => this.setData(data));
        applicationModel.onDataInvalidate.listen(data => this.invalidateData());
        applicationModel.isOffline.listen(isOffline => {
            if (isOffline) {
                this.goOffline()
            }
        });
    }

    start() {
        this.updateData();
    }

    private goOffline() {
        this.resetTimer();
    }

    private invalidateData() {
        this.resetTimer();
        this.updateData();
    }

    private setData(data: SchedulerData) {
        this.resetTimer();

        const
            nextUpdateDate = this.calculateNextUpdateDate(data),
            sleepInterval = this.calculateSleepInterval(nextUpdateDate);

        this.scheduleUpdateIn(sleepInterval);
    }

    /**
     * 计划恢复
     */
    private scheduleRecovery() {
        this.scheduleUpdateIn(DataLoader.DEFAULT_UPDATE_INTERVAL);
    }

    private scheduleUpdateIn(sleepInterval) {
        const now = new Date(),
            actualUpdateDate = new Date(now.getTime() + sleepInterval),
            message = 'next update at ' + actualUpdateDate.toTimeString();

        this.applicationModel.autoUpdateMessage.setValue(message);

        this._autoUpdateTimer.schedule(() => {
            this.updateData();
        }, sleepInterval);
    }

    /**
     * 重置时间
     */
    private resetTimer() {
        this._autoUpdateTimer.reset();
    }

    /**
     * 计算睡眠间隔
     * @param nextUpdateDate
     */
    private calculateSleepInterval(nextUpdateDate: Date) {
        var now = new Date(),
            sleepInterval = nextUpdateDate.getTime() - now.getTime();

        if (sleepInterval < 0) {
            // UpdateDate 在过去，调度程序可能尚未启动
            return DataLoader.DEFAULT_UPDATE_INTERVAL;
        }

        if (sleepInterval < DataLoader.MIN_UPDATE_INTERVAL) {
            //延迟间隔太小
            //我们需要扩展它以避免大量的查询
            return DataLoader.MIN_UPDATE_INTERVAL;
        }

        if (sleepInterval > DataLoader.MAX_UPDATE_INTERVAL) {
            //间隔太大
            return DataLoader.MAX_UPDATE_INTERVAL;
        }

        return sleepInterval;
    }

    /**
     * 修改时间
     */
    private updateData() {
        this.applicationModel.autoUpdateMessage.setValue('updating...');
        this.commandService
            .executeCommand(new GetDataCommand())
            .fail((error: ErrorInfo) => {
                if (!error.disconnected) {
                    this.scheduleRecovery();
                }

                //如果服务器不可用，我们不计划恢复，因为这应该通过脱机模式屏幕完成。
            })
            .done((data) => {
                this.applicationModel.setData(data);
            });
    }

    /**
     * 获取默认的修改时间
     */
    private getDefaultUpdateDate() {
        var now = new Date();
        now.setSeconds(now.getSeconds() + 30);
        return now;
    }

    /**
     * 获取上一次的活动时间
     * @param data
     */
    private getLastActivityFireDate(data: SchedulerData): Date {
        if (data.Status !== 'started') {
            return null;
        }

        const
            allJobs = __flatten(__map(data.JobGroups, group => group.Jobs)),
            allTriggers = __flatten(__map(allJobs, (job: Job) => job.Triggers)),
            activeTriggers = __filter(allTriggers, (trigger: Trigger) => trigger.Status === ActivityStatus.Active),
            nextFireDates = __compact(__map(activeTriggers, (trigger: Trigger) => trigger.NextFireDate == null ? null : trigger.NextFireDate));

        return nextFireDates.length > 0 ? new Date(__min(nextFireDates)) : null;
    }

    /**
     * 获取基于立即执行的更新日期
     * @param data
     */
    private getExecutingNowBasedUpdateDate(data: SchedulerData): Date {
        if (data.InProgress && data.InProgress.length > 0) {
            return this.nowPlusMilliseconds(DataLoader.DEFAULT_UPDATE_INTERVAL_IN_PROGRESS);
        }

        return null;
    }

    /**
     * 计算下一个更新日期
     * @param data
     */
    private calculateNextUpdateDate(data: SchedulerData): Date {
        const
            inProgressBasedUpdateDate = this.getExecutingNowBasedUpdateDate(data),
            triggersBasedUpdateDate = this.getLastActivityFireDate(data) || this.getDefaultUpdateDate();

        if (inProgressBasedUpdateDate && triggersBasedUpdateDate.getTime() > inProgressBasedUpdateDate.getTime()) {
            return inProgressBasedUpdateDate;
        }

        return triggersBasedUpdateDate;
    }

    private nowPlusMilliseconds(value: number) {
        return new Date(new Date().getTime() + value);
    }
}