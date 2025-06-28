public class Constants
{
public static string ARGUMENTS = string.Join(" ",
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
    "--dpi-desync=split",
    "--dpi-desync-split-pos=1",
    "--dpi-desync-autottl",
    "--dpi-desync-fooling=badseq",
    "--dpi-desync-repeats=8");


    public const string TASK_NAME = "zapretAutoStart";
    private static string BIT_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zapret", "bin");
    public static string EXE_PATH = Path.Combine(BIT_PATH, "winws.exe");
    public static string EXCEPTIONS_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zapret", "list-general.txt");
}