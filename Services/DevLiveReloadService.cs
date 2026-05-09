namespace AisVacanciesAndResumes.Services;

public sealed class DevLiveReloadService : BackgroundService
{
    private static readonly string[] WatchedExtensions = [".cshtml", ".css", ".js"];
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DevLiveReloadService> _logger;
    private readonly List<FileSystemWatcher> _watchers = [];
    private readonly object _changeLock = new();
    private TaskCompletionSource<long> _changeSignal = NewChangeSignal();
    private long _version;
    private DateTime _lastChangeUtc = DateTime.MinValue;

    public DevLiveReloadService(IWebHostEnvironment environment, ILogger<DevLiveReloadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public long Version => Interlocked.Read(ref _version);

    public async Task<long> WaitForChangeAsync(long knownVersion, CancellationToken cancellationToken)
    {
        if (Version > knownVersion)
        {
            return Version;
        }

        Task<long> waitTask;

        lock (_changeLock)
        {
            if (Version > knownVersion)
            {
                return Version;
            }

            waitTask = _changeSignal.Task;
        }

        try
        {
            return await waitTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return knownVersion;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        WatchDirectory(Path.Combine(_environment.ContentRootPath, "Views"));
        WatchDirectory(Path.Combine(_environment.ContentRootPath, "wwwroot", "css"));
        WatchDirectory(Path.Combine(_environment.ContentRootPath, "wwwroot", "js"));

        stoppingToken.Register(() =>
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
        });

        return Task.CompletedTask;
    }

    private void WatchDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var watcher = new FileSystemWatcher(path)
        {
            EnableRaisingEvents = true,
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
        };

        watcher.Changed += OnFileChanged;
        watcher.Created += OnFileChanged;
        watcher.Deleted += OnFileChanged;
        watcher.Renamed += OnFileRenamed;
        _watchers.Add(watcher);
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        PublishChange(e.FullPath);
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        PublishChange(e.FullPath);
    }

    private void PublishChange(string path)
    {
        if (!WatchedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (now - _lastChangeUtc < TimeSpan.FromMilliseconds(250))
        {
            return;
        }

        _lastChangeUtc = now;
        var version = Interlocked.Increment(ref _version);
        TaskCompletionSource<long> signal;
        lock (_changeLock)
        {
            signal = _changeSignal;
            _changeSignal = NewChangeSignal();
        }

        signal.TrySetResult(version);
        _logger.LogDebug("Live reload change detected: {Path}", path);
    }

    private static TaskCompletionSource<long> NewChangeSignal()
    {
        return new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
