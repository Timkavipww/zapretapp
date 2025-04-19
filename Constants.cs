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
        "--dpi-desync=fake,split",
        "--dpi-desync-autottl=2",
        "--dpi-desync-repeats=6",
        "--dpi-desync-fooling=badseq",
        $"--dpi-desync-fake-tls=\"{Path.Combine("bin", "tls_clienthello_www_google_com.bin")}\"");
    public const string TASK_NAME = "zapretAutoStart";
    private static string BIT_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zapret", "bin");
    public static string EXE_PATH = Path.Combine(BIT_PATH, "winws.exe");

}