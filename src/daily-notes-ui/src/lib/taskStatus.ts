export const TASK_STATUS = {
    pending: 'pending',
    inProgress: 'in_progress',
    completed: 'completed',
    onHold: 'on_hold',
} as const;

export type TaskStatus = typeof TASK_STATUS[keyof typeof TASK_STATUS];

export const TASK_STATUS_OPTIONS = [
    'all',
    TASK_STATUS.pending,
    TASK_STATUS.inProgress,
    TASK_STATUS.completed,
    TASK_STATUS.onHold,
] as const;

export type TaskStatusFilter = typeof TASK_STATUS_OPTIONS[number];
