SELECT "Status",
    COUNT(*)
FROM work_tasks
GROUP BY "Status";