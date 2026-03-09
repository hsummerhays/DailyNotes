SELECT CASE
        WHEN max_note < '2021-03-09' THEN 'older than 5y'
        ELSE 'recent'
    END as status_period,
    COUNT(*)
FROM (
        SELECT "WorkTaskId",
            MAX("NoteDate") as max_note
        FROM work_notes
        WHERE "WorkTaskId" IS NOT NULL
        GROUP BY "WorkTaskId"
    ) as task_max_dates
GROUP BY status_period;
SELECT COUNT(*) as tasks_with_no_notes
FROM work_tasks
WHERE "Id" NOT IN (
        SELECT DISTINCT "WorkTaskId"
        FROM work_notes
        WHERE "WorkTaskId" IS NOT NULL
    );