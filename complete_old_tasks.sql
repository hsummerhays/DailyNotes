BEGIN;
UPDATE work_tasks
SET "Status" = 'completed',
    "UpdatedAt" = NOW()
WHERE (
        "Id" NOT IN (
            SELECT DISTINCT "WorkTaskId"
            FROM work_notes
            WHERE "WorkTaskId" IS NOT NULL
        )
        OR "Id" IN (
            SELECT "WorkTaskId"
            FROM work_notes
            WHERE "WorkTaskId" IS NOT NULL
            GROUP BY "WorkTaskId"
            HAVING MAX("NoteDate") < '2021-03-09'
        )
    )
    AND "Status" != 'completed';
SELECT "Status",
    COUNT(*)
FROM work_tasks
GROUP BY "Status";
COMMIT;