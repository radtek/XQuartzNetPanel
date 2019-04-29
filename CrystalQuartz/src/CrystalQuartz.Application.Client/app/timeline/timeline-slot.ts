import {
    IRange,
    ITimelineSlotOptions,
    ITimelineActivityOptions,
    ActivityInteractionRequest
} from './common';

import TimelineActivity from './timeline-activity';

export default class TimelineSlot {
    activities = new js.ObservableList<TimelineActivity>();

    key: string;

    constructor(options:ITimelineSlotOptions) {
        this.key = options.key;
    }

    /**
     * 添加活动
     * @param activity
     * @param selectionRequestCallback
     */
    add(activity:ITimelineActivityOptions, selectionRequestCallback: (activity: TimelineActivity, requestType: ActivityInteractionRequest) => void) {
        const result = new TimelineActivity(activity, requestType => selectionRequestCallback(result, requestType));
        this.activities.add(result);

        return result;
    };

    /**
     * 移除活动
     * @param activity
     */
    remove(activity: TimelineActivity) {
        this.activities.remove(activity);
    };

    /**
     * 从槽中删除所有活动
     */
    clear() {
        this.activities.clear();
    }

    /**
     * 是否为空
     */
    isEmpty() {
        return this.activities.getValue().length === 0;
    };

    /**
     * 是否是 繁忙的
     */
    isBusy() {
        return !!this.findCurrentActivity();
    }

    recalculate(range: IRange) : { isEmpty: boolean, removedActivities: TimelineActivity[] } {
        const activities = this.activities.getValue(),
              rangeStart = range.start,
              rangeEnd = range.end,
              removed = [];

        for (let i = 0; i < activities.length; i++) {
            const activity = activities[i];

            if (!activity.recalculate(rangeStart, rangeEnd)) {
                this.activities.remove(activity);
                removed.push(activity);
            }
        }

        return {
            isEmpty: this.isEmpty(),
            removedActivities: removed
        };
    }

    /**
     * 寻找活动
     * @param key
     */
    findActivityBy(key: string) {
        const activities = this.activities.getValue();
        for (let i = 0; i < activities.length; i++) {
            if (activities[i].key === key) {
                return activities[i];
            }
        }

        return null;
    }

    /**
     * 请求当前活动详细信息
     */
    requestCurrentActivityDetails() {
        const currentActivity = this.findCurrentActivity();
        if (currentActivity) {
            currentActivity.requestDetails();
        }
    }

    /**
     * 查找当前活动
     */
    private findCurrentActivity(): TimelineActivity {
        const activities = this.activities.getValue();

        for (var i = activities.length - 1; i >= 0; i--) {
            if (!activities[i].completedAt) {
                return activities[i];
            }
        }

        return null;
    }
};