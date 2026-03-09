SELECT COUNT(*)
FROM work_tasks
WHERE "Status" != 'completed'
    AND "Id" NOT IN (
        SELECT DISTINCT "WorkTaskId"
        FROM work_notes
        WHERE "NoteDate" >= '2021-03-09'
            AND "WorkTaskId" IS NOT NULL
    )
    AND "CreatedAt" < '2021-03-09';