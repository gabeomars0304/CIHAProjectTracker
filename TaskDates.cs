public static class TaskDates
{
    public static void Apply(ItemTask task, bool completed, DateTime? inheritedEndDate = null)
    {
        task.IsCompleted = completed;

        if (completed)
        {
            // Keep an existing end date; otherwise inherit from the parent, then fall back to today
            task.EndDate ??= inheritedEndDate ?? DateTime.Today;
        }
        else
        {
            task.EndDate = null;
        }

        foreach (var sub in task.SubTasks)
            Apply(sub, completed, task.EndDate);
    }

    public static bool SetDerivedStatus(ItemTask task, bool completed, DateTime? fallbackEndDate = null)
    {
        if (task.IsCompleted == completed) return false;

        task.IsCompleted = completed;
        if (completed)
            task.EndDate ??= fallbackEndDate ?? DateTime.Today;
        else
            task.EndDate = null;

        return true;
    }
}