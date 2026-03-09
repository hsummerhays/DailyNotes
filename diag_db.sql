SELECT MAX("NoteDate") as max_note_date,
    MIN("NoteDate") as min_note_date,
    COUNT(*) as total_notes
FROM work_notes;
SELECT MAX("CreatedAt") as max_task_created,
    MIN("CreatedAt") as min_task_created,
    COUNT(*) as total_tasks
FROM work_tasks;
SELECT "Status",
    COUNT(*)
FROM work_tasks
GROUP BY "Status";