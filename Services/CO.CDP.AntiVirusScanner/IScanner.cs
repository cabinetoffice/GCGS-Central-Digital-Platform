namespace CO.CDP.AntiVirusScanner;

public interface IScanner
{
    void Scan(ScanFile fileName);
}