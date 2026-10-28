using Microsoft.EntityFrameworkCore;

public enum SaveResult { Ok, Conflict, Deleted }

public sealed class EditModel
{
    public bool IsItem { get; init; }
    public int Id { get; init; }
    public byte[] RowVersion { get; set; } = [];

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCompleted { get; set; }

    //items only
    public bool IsGloballyTracked { get; set; }
    public List<string> TagNames { get; set; } = [];
}

public sealed class ItemService(IDbContextFactory<AppDbContext> factory, ChangeNotifier notifier)
{
    //---------- reads ----------
    public async Task<(List<TrackedItem> Items, List<ItemTag> Tags)> LoadAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var tags = await db.Tags.OrderBy(t => t.Name).ToListAsync();
        var items = await db.Items.Include(i => i.Tags).OrderBy(i => i.Id).ToListAsync();
        await db.Tasks.OrderBy(t => t.Id).ToListAsync();   //fix-up builds Tasks/SubTasks
        return (items, tags);   //detached once the context is disposed, graph stays intact
    }

    public async Task<EditModel?> GetEditModelAsync(bool isItem, int id)
    {
        await using var db = await factory.CreateDbContextAsync();

        if (isItem)
        {
            var item = await db.Items.AsNoTracking().Include(i => i.Tags)
                               .FirstOrDefaultAsync(i => i.Id == id);
            return item is null ? null : new EditModel
            {
                IsItem = true,
                Id = item.Id,
                RowVersion = item.RowVersion,
                Title = item.Title,
                Description = item.Description,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                IsCompleted = item.IsCompleted,
                IsGloballyTracked = item.IsGloballyTracked,
                TagNames = item.Tags.Select(tag => tag.Name).ToList()
            };
        }

        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return task is null ? null : new EditModel
        {
            IsItem = false,
            Id = task.Id,
            RowVersion = task.RowVersion,
            Title = task.Title,
            Description = task.Description,
            StartDate = task.StartDate,
            EndDate = task.EndDate,
            IsCompleted = task.IsCompleted
        };
    }

    //Current row version, or null if the row no longer exists.
    public async Task<byte[]?> GetRowVersionAsync(bool isItem, int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return isItem
            ? await db.Items.Where(i => i.Id == id).Select(i => i.RowVersion).FirstOrDefaultAsync()
            : await db.Tasks.Where(t => t.Id == id).Select(t => t.RowVersion).FirstOrDefaultAsync();
    }

    //Tags creation/deletion
    public async Task<List<ItemTag>> GetTagsAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Tags.AsNoTracking().OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<int> CountTagUsageAsync(int tagId)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Tags.Where(t => t.Id == tagId).SelectMany(t => t.Items).CountAsync();
    }

    //Creates the tag, or return the existing one if the name is already taken
    public async Task<ItemTag> CreateTagAsync(string name, string? colorHex = null)
    {
        name = name.Trim();
        if (colorHex is not null && !System.Text.RegularExpressions.Regex.IsMatch(colorHex, "^#[0-9A-Fa-f]{6}$"))
            colorHex = null;

        await using (var db = await factory.CreateDbContextAsync())
        {
            //SQL Server's default collation makes this comparison case-insensitive
            var existing = await db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Name == name);
            if (existing is not null) return existing;

            var tag = new ItemTag { Name = name, ColorHex = colorHex };
            db.Tags.Add(tag);
            try
            {
                await db.SaveChangesAsync();
                notifier.Publish();
                return tag;
            }
            catch (DbUpdateException)
            {
                //Someone most likely created the same tag at the same time: fall through and read it
            }
        }

        await using var db2 = await factory.CreateDbContextAsync();
        return await db2.Tags.AsNoTracking().FirstAsync(t => t.Name == name);
    }

    //Deletes the tag and removes it from every item that had it.
    public async Task DeleteTagAsync(int tagId)
    {
        for (var attempt =0; attempt < 3; attempt++)
        {
            await using var db = await factory.CreateDbContextAsync();
            var tag = await db.Tags.Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag is null) return;    //Deleted by someone else

            //Touch each affected item sot its RowVersion changes. Anyone with that item open
            //in the edit dialog gets the "Changed by someone else" banner instead of
            //silently re-creating the tag when they save.
            foreach (var item in tag.Items)
                db.Entry(item).Property(i => i.Title).IsModified = true;
            db.Tags.Remove(tag);    //join rows are removed by the cascade

            try
            {
                await db.SaveChangesAsync();
                notifier.Publish();
                return;
            } catch (DbUpdateConcurrencyException) { /* an item changed mid-delete: retry with fresh data */ }
        }
    }

    //---------- writes ----------
    public async Task<SaveResult> SaveAsync(EditModel m)
    {
        await using var db = await factory.CreateDbContextAsync();
        var items = await db.Items.Include(i => i.Tags).ToListAsync();
        var tasks = await db.Tasks.ToListAsync();

        if (m.IsItem)
        {
            var item = items.FirstOrDefault(i => i.Id == m.Id);
            if (item is null) return SaveResult.Deleted;

            //Make EF check against the version the user started from, not the one just loaded
            db.Entry(item).Property(i => i.RowVersion).OriginalValue = m.RowVersion;

            var wasCompleted = item.IsCompleted;
            item.Title = m.Title;
            item.Description = m.Description;
            item.StartDate = m.StartDate;
            item.EndDate = m.EndDate;
            item.IsCompleted = m.IsCompleted;
            item.IsGloballyTracked = m.IsGloballyTracked;
            await SyncTagsAsync(db, item, m.TagNames);

            if (wasCompleted != m.IsCompleted)
                foreach (var child in item.Tasks)
                    TaskDates.Apply(child, item.IsCompleted, item.EndDate);
        }
        else
        {
            var task = tasks.FirstOrDefault(t => t.Id == m.Id);
            if (task is null) return SaveResult.Deleted;

            db.Entry(task).Property(t => t.RowVersion).OriginalValue = m.RowVersion;

            var wasCompleted = task.IsCompleted;
            task.Title = m.Title;
            task.Description = m.Description;
            task.StartDate = m.StartDate;
            task.EndDate = m.EndDate;
            task.IsCompleted = m.IsCompleted;

            if (wasCompleted != m.IsCompleted)
            {
                TaskDates.Apply(task, m.IsCompleted, m.EndDate);   //cascades to subtasks
                if (!m.IsCompleted) task.EndDate = m.EndDate;      //keep a future date the user chose
            }
        }

        RecomputeAll(items);
        return await TrySaveAsync(db);
    }

    public async Task AddItemAsync(string title, string? description, DateTime? start, IEnumerable<string> tagNames, bool tracked)
    {
        await using var db = await factory.CreateDbContextAsync();
        var item = new TrackedItem
        {
            Title = title,
            Description = description ?? string.Empty,
            StartDate = start,
            IsGloballyTracked = tracked
        };
        db.Items.Add(item);
        await SyncTagsAsync(db, item, tagNames.ToList());
        await TrySaveAsync(db);
    }

    public async Task AddTaskAsync(int? itemId, int? parentTaskId, string title, string? description, DateTime? start)
    {
        await using var db = await factory.CreateDbContextAsync();
        var items = await db.Items.ToListAsync();
        await db.Tasks.ToListAsync();

        db.Tasks.Add(new ItemTask
        {
            Title = title,
            Description = description ?? string.Empty,
            StartDate = start,
            TrackedItemId = itemId,
            ParentId = parentTaskId
        });

        RecomputeAll(items);   //a new unfinished child can un-complete its parents
        await TrySaveAsync(db);
    }

    public async Task DeleteAsync(bool isItem, int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var items = await db.Items.ToListAsync();
        var tasks = await db.Tasks.ToListAsync();   //loaded so ClientCascade can delete subtasks

        if (isItem)
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            if (item is null) return;
            db.Items.Remove(item);
        }
        else
        {
            var task = tasks.FirstOrDefault(t => t.Id == id);
            if (task is null) return;

            if (task.ParentId is int parentId)
                tasks.FirstOrDefault(p => p.Id == parentId)?.SubTasks.Remove(task);
            else
                items.FirstOrDefault(i => i.Id == task.TrackedItemId)?.Tasks.Remove(task);

            db.Tasks.Remove(task);
            RecomputeAll(items);
        }

        await TrySaveAsync(db);
    }

    //---------- helpers ----------
    private async Task<SaveResult> TrySaveAsync(AppDbContext db)
    {
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return SaveResult.Conflict; }

        notifier.Publish();
        return SaveResult.Ok;
    }

    private static async Task SyncTagsAsync(AppDbContext db, TrackedItem item, List<string> names)
    {
        var wanted = names.Select(n => n.Trim()).Where(n => n.Length > 0)
                          .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var all = await db.Tags.ToListAsync();

        item.Tags.RemoveAll(tag => !wanted.Contains(tag.Name, StringComparer.OrdinalIgnoreCase));

        foreach (var name in wanted)
        {
            if (item.Tags.Any(tag => string.Equals(tag.Name, name, StringComparison.OrdinalIgnoreCase)))
                continue;

            item.Tags.Add(all.FirstOrDefault(tag => string.Equals(tag.Name, name, StringComparison.OrdinalIgnoreCase))
                          ?? new ItemTag { Name = name });
        }
    }

    //---- unchanged ----
    private static void RecomputeAll(IEnumerable<TrackedItem> items)
    {
        foreach (var item in items) RecomputeItem(item);
    }

    private static bool RecomputeTaskStatus(ItemTask task)
    {
        if (task.SubTasks.Count == 0) return task.IsCompleted;

        bool allChildrenComplete = true;
        foreach (var sub in task.SubTasks)
            if (!RecomputeTaskStatus(sub)) allChildrenComplete = false;

        TaskDates.SetDerivedStatus(task, allChildrenComplete, task.SubTasks.Max(s => s.EndDate));
        return allChildrenComplete;
    }

    private static void RecomputeItem(TrackedItem item)
    {
        if (item.Tasks.Count == 0) return;

        bool allComplete = true;
        foreach (var task in item.Tasks)
            if (!RecomputeTaskStatus(task)) allComplete = false;

        if (item.IsCompleted != allComplete)
        {
            item.IsCompleted = allComplete;
            if (allComplete)
                item.EndDate ??= item.Tasks.Max(t => t.EndDate) ?? DateTime.Today;
            else
                item.EndDate = null;
        }
    }
}