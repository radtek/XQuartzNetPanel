import {JobGroup} from './api';

/**
 * 提供存取的两种基本数据 : groups, jobs, triggers.
 */
export interface SchedulerExplorer {
    listGroups(): JobGroup[];
}