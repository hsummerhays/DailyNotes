SELECT CASE
        WHEN max_note < '2023-03-09' THEN 'older than 3y'
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