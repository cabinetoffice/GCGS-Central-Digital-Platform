using CO.CDP.AntiVirusScanner;
using CO.CDP.MQ;

namespace CO.CDP.AntiVirusScanner;

public class ScanFileSubscriber(IScanner scanner)
    : ISubscriber<ScanFile>
{
    public async Task Handle(ScanFile @event)
    {
        await scanner.Scan(@event);
    }
}