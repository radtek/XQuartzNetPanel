import { CommandService, ErrorInfo } from '../services';
import { GetEnvironmentDataCommand, GetDataCommand } from '../commands/global-commands';
import { SchedulerData, EnvironmentData, SchedulerStatus } from '../api';
import { ApplicationModel } from '../application-model';
import { DataLoader } from '../data-loader';
import ApplicationViewModel from '../application-view-model';

import { DefaultNotificationService } from '../notification/notification-service';
import { RetryTimer } from "../global/timers/retry-timer";

import __each from 'lodash/each';
import { TimelineInitializer } from "../timeline/timeline-initializer";
import DateUtils from "../utils/date";

export enum FaviconStatus {
    Loading,
    Ready,
    Active,
    Broken
}

export default class BootstrapperViewModel {
    statusMessage = new js.ObservableValue<string>();
    status = new js.ObservableValue<boolean>();
    favicon = new js.ObservableValue<FaviconStatus>();
    title = new js.ObservableValue<string>();
    failed = new js.ObservableValue<boolean>();
    errorMessage = new js.ObservableValue<string>();
    retryIn = new js.ObservableValue<string>();
    customStylesUrl = new js.ObservableValue<string>();

    applicationViewModel: ApplicationViewModel;

    private _currentTimer: RetryTimer<any>;

    private _commandService: CommandService;
    private _applicationModel: ApplicationModel;
    private _notificationService: DefaultNotificationService;
    private _dataLoader: DataLoader;
    private _timelineInitializer: TimelineInitializer;
    private _initialData: SchedulerData;

    start() {
        this._commandService = new CommandService(),
            this._applicationModel = new ApplicationModel(),
            this._notificationService = new DefaultNotificationService(),
            this._dataLoader = new DataLoader(this._applicationModel, this._commandService);

        this.initialSetup();
        this.performLoading();
    }

    /**
     * 应用程序渲染
     */
    onAppRendered() {
        this.setupFaviconListeners();

        //设置 页面的Title
        js.dependentValue(
            (isOffline: boolean, schedulerName: string, inProgressCount: number) => {
                /**
                 * 页面title
                 */
                if (isOffline) {
                    return (schedulerName ? schedulerName + ' - ' : '') + '断开 since ' + DateUtils.smartDateFormat(this._applicationModel.offlineSince);
                }

                const suffix = inProgressCount == 0 ? '' : ` - ${inProgressCount} ${inProgressCount === 1 ? 'job' : 'jobs'} in progress`;

                return schedulerName + suffix;
            },
            this._applicationModel.isOffline,
            this._applicationModel.schedulerName,
            this._applicationModel.inProgressCount
        ).listen(composedTitle => this.title.setValue(composedTitle));

        this._initialData = null;
    }

    /**
     * 初始化 设置
     */
    private initialSetup() {
        this.favicon.setValue(FaviconStatus.Loading);
        this.title.setValue('加载中...');
    }

    private setupFaviconListeners() {
        this._applicationModel.isOffline.listen(isOffline => {
            if (isOffline) {
                this.favicon.setValue(FaviconStatus.Broken);
            }
        })

        const syncFaviconWithSchedulerData = (data: SchedulerData) => {
            if (data) {
                const schedulerStatus = SchedulerStatus.findByCode(data.Status);

                if (schedulerStatus === SchedulerStatus.Started) {
                    this.favicon.setValue(FaviconStatus.Active);
                } else {
                    this.favicon.setValue(FaviconStatus.Ready);
                }
            }
        };

        this._applicationModel.onDataChanged.listen(syncFaviconWithSchedulerData);
        syncFaviconWithSchedulerData(this._initialData);

    }

    private performLoading() {
        const
            stepEnvironment = this.wrapWithRetry(
                () => {
                    this.statusMessage.setValue('加载环境设置..');
                    return this._commandService.executeCommand<EnvironmentData>(new GetEnvironmentDataCommand());
                }),

            stepData = stepEnvironment.then(
                (envData: EnvironmentData) => this.wrapWithRetry(
                    () => {
                        /**
                         *我们需要在第一次呼叫之前初始化时间线
                         *以获取用于处理此调用中的事件的数据方法。
                         */
                        this._timelineInitializer = new TimelineInitializer(envData.TimelineSpan);
                        this._timelineInitializer.start(this._commandService.onEvent);

                        if (envData.CustomCssUrl) {
                            this.statusMessage.setValue('加载自定义样式..');
                            this.customStylesUrl.setValue((envData.CustomCssUrl));
                        }

                        this.statusMessage.setValue('正在加载调度器数据..');

                        return this._commandService.executeCommand<SchedulerData>(new GetDataCommand()).then(schedulerData => {
                            this.statusMessage.setValue('完成..');

                            return {
                                envData: envData,
                                schedulerData: schedulerData
                            };
                        });
                    }
                ));

        stepData.done(data => {
            this.applicationViewModel = new ApplicationViewModel(
                this._applicationModel,
                this._commandService,
                data.envData,
                this._notificationService,
                this._timelineInitializer);

            this._initialData = data.schedulerData;

            /**
             * 这将触发应用程序服务。
             */
            this._applicationModel.setData(data.schedulerData);
            this.status.setValue(true);
        });
    }

    /**
     * 重试
     * @param payload
     */
    private wrapWithRetry<T>(payload: () => JQueryPromise<T>): JQueryPromise<T> {
        const
            errorHandler = (error: ErrorInfo) => {
                this.failed.setValue(true);
                this.errorMessage.setValue(error.errorMessage);
            },

            actualPayload = (isRetry: boolean) => {
                this.failed.setValue(false);

                if (isRetry) {
                    this.statusMessage.setValue('重试...');
                }

                return payload();
            },

            timer = new RetryTimer(actualPayload, 5, 60, errorHandler),

            disposables = [
                timer.message.listen(message => this.retryIn.setValue(message)),
                timer
            ];

        this._currentTimer = timer;

        return timer
            .start(false)
            .always(() => {
                __each(disposables, x => x.dispose());
            });
    }

    /**
     * 取消自动重试
     */
    cancelAutoRetry() {
        if (this._currentTimer) {
            this._currentTimer.reset();
        }

        this.retryIn.setValue('取消');
    }

    /**
     * 立即重试
     */
    retryNow() {
        if (this._currentTimer) {
            this._currentTimer.force();
        }
    }
}