using System.Diagnostics;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

public class CustomContext : ApplicationContext
{
    string arguments = string.Join(" ",
        "--wf-tcp=80,443",
        "--wf-udp=443,50000-50100",
        "--filter-udp=443",
        "--hostlist=\"list-general.txt\"",
        "--dpi-desync=fake",
        "--dpi-desync-repeats=6",
        $"--dpi-desync-fake-quic=\"{Path.Combine("bin", "quic_initial_www_google_com.bin")}\"",
        "--new",
        "--filter-udp=50000-50100",
        "--ipset=\"ipset-discord.txt\"",
        "--dpi-desync=fake",
        "--dpi-desync-any-protocol",
        "--dpi-desync-cutoff=d3",
        "--dpi-desync-repeats=6",
        "--new",
        "--filter-tcp=80",
        "--hostlist=\"list-general.txt\"",
        "--dpi-desync=fake,split2",
        "--dpi-desync-autottl=2",
        "--dpi-desync-fooling=md5sig",
        "--new",
        "--filter-tcp=443",
        "--hostlist=\"list-general.txt\"",
        "--dpi-desync=fake,split",
        "--dpi-desync-autottl=2",
        "--dpi-desync-repeats=6",
        "--dpi-desync-fooling=badseq",
        $"--dpi-desync-fake-tls=\"{Path.Combine("bin", "tls_clienthello_www_google_com.bin")}\""
    );
    NotifyIcon trayIcon;
    Process? runningProcess;
    Icon startedIcon = new Icon(Path.Combine(Application.StartupPath, "assets", "icon-green.ico"));
    Icon stoppedIcon = new Icon(Path.Combine(Application.StartupPath, "assets", "icon-red.ico"));

    ContextMenuStrip contextMenu = new();
    ToolStripMenuItem statusItem;
    public CustomContext()
    {

        trayIcon = new NotifyIcon()
        {
            Text = "Статус: " + GetStatus(),
            Visible = true,
        };

        statusItem = new ToolStripMenuItem("Статус: " + GetStatus())
        {
            Enabled = false,
            ForeColor = Color.DarkGreen
        };
        contextMenu.Items.Add(statusItem);
        contextMenu.Items.Add(new ToolStripMenuItem("Запустить", null, (s, e) => StartBat()));
        contextMenu.Items.Add(new ToolStripMenuItem("Остановить", null, (s, e) => StopBat()));
        contextMenu.Items.Add(new ToolStripMenuItem("Выход", null, (s, e) =>
        {
            trayIcon.Visible = false;
            StopBatWithoutMessage();
            ExitThread();
        }));

        trayIcon.ContextMenuStrip = contextMenu;
        StartBat();
        UpdateStatus();
        AddToTaskScheduler();
    }

    private string GetStatus()
    {
        return (runningProcess != null && !runningProcess.HasExited) ? "Запущен" : "Остановлен";
    }

    private void UpdateStatus()
    {
        string statusText = GetStatus();
         trayIcon.Text = "Статус: " + statusText;
    
        if (statusItem != null)
            statusItem.Text = "Статус: " + statusText;

        bool isRunning = runningProcess != null && !runningProcess.HasExited;

        trayIcon.Icon = isRunning ? startedIcon : stoppedIcon;

    }

    private void StartBat()
    {
        if (runningProcess != null && !runningProcess.HasExited)
    {
        MessageBox.Show("Уже запущено", "Инфо");
        return;
    }

    string binPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zapret", "bin");
    string exePath = Path.Combine(binPath, "winws.exe");

    if (!File.Exists(exePath))
    {
        MessageBox.Show("winws.exe не найден", "Ошибка");
        return;
    }

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zapret"),
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        try
        {
            runningProcess = Process.Start(psi);
            UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при запуске: {ex.Message}", "Ошибка");
        }
    }


    private void StopBat()
    {
        if (runningProcess == null || runningProcess.HasExited)
        {
            MessageBox.Show("Скрипт не запущен", "Инфо");
            return;
        }

        try
        {
            runningProcess.Kill();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка остановки: " + ex.Message, "Ошибка");
        }
        
        UpdateStatus();
    }
    private void StopBatWithoutMessage()
    {
        try
        {
            runningProcess?.Kill();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка остановки: " + ex.Message, "Ошибка");
        }
    }
    public static void AddToTaskScheduler()
    {
        string taskName = "zapretAutoStart";
        string exePath = Application.ExecutablePath;

        using (TaskService ts = new TaskService())
        {
            TaskDefinition td = ts.NewTask();
            td.RegistrationInfo.Description = "Запуск zapretapp с правами администратора при входе";
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.RunOnlyIfNetworkAvailable = false;
            td.Settings.StartWhenAvailable = true;
            td.Settings.AllowHardTerminate = false;

            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Principal.LogonType = TaskLogonType.Password;

            td.Triggers.Add(new LogonTrigger());

            td.Actions.Add(new ExecAction(exePath, null, null));    
            ts.RootFolder.RegisterTaskDefinition(taskName, td, TaskCreation.CreateOrUpdate, Environment.UserName, null, TaskLogonType.InteractiveToken);
        }
    }

}