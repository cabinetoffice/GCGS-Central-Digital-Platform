namespace CO.CDP.AntiVirusScanner;

public interface IScanner
{
    Task Scan(ScanFile fileName);
}