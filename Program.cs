static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CustomContext());
        }
        catch (Exception ex)
        {
            File.WriteAllText("errorlog.txt", ex.ToString());
            MessageBox.Show("Ошибка: " + ex.Message);
        }
    }
}
