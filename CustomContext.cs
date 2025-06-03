public class CustomContext : ApplicationContext
{
    NotifyIcon trayIcon;
    Process? runningProcess;
    Icon startedIcon = new Icon(Path.Combine(Application.StartupPath, "assets", "icon-green.ico"));
    Icon stoppedIcon = new Icon(Path.Combine(Application.StartupPath, "assets", "icon-red.ico"));
    ContextMenuStrip contextMenu = new();
    ToolStripMenuItem additionalMenu = new("Дополнительно");
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
        contextMenu.Items.Add(new ToolStripMenuItem("Удалить из планировщика", null, (s, e) => {
            RemoveFromTaskScheduler();
            trayIcon.Visible = false;
            StopBatWithoutMessage();
            ExitThread();
        }));
        
        additionalMenu.DropDownItems.Add("Версия", null, (s, e) =>
        {
            MessageBox.Show("Версия 1.0.1", "О программе");
        });

        additionalMenu.DropDownItems.Add("Добавить исключения", null, (s, e) =>
        {
            string promptText = "Введите текст исключения:" + Environment.NewLine + "Пример: example.com";
            string input = Microsoft.VisualBasic.Interaction.InputBox(promptText, "Добавить исключение", "");
            MessageBox.Show(input, "Инфо");
            if (!string.IsNullOrWhiteSpace(input))
            {
                AddExceptions(input);
            }
        });
        
        contextMenu.Items.Add(additionalMenu);
        
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

    private void AddExceptions(string text)
    {
        File.AppendAllText(Constants.EXCEPTIONS_PATH, text + Environment.NewLine);
        StopBatWithoutMessage();
        Thread.Sleep(2000);
        StartBat();
    }

    private void StartBat()
    {
        if (runningProcess != null && !runningProcess.HasExited)
        {
            MessageBox.Show("Уже запущено", "Инфо");
            return;
        }



        if (!File.Exists(Constants.EXE_PATH))
        {
            MessageBox.Show("winws.exe не найден", "Ошибка");
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = Constants.EXE_PATH,
            Arguments = Constants.ARGUMENTS,
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
    private static void AddToTaskScheduler()
    {
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
            ts.RootFolder.RegisterTaskDefinition(Constants.TASK_NAME, td, TaskCreation.CreateOrUpdate, Environment.UserName, null, TaskLogonType.InteractiveToken);
        }
    }
    private static void RemoveFromTaskScheduler()
    {
        using (TaskService ts = new TaskService())
        {
            try
            {
                ts.RootFolder.DeleteTask(Constants.TASK_NAME, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}");
            }
        }
    }
}