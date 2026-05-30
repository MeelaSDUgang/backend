using System.Threading.Channels;

namespace ComplianceDashboard.Services.TransactionProcessing;

public class TransactionWorkerQueue : ITransactionWorkerQueue
{
    private readonly Channel<TransactionWorkItem> _queue = Channel.CreateUnbounded<TransactionWorkItem>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(TransactionWorkItem item, CancellationToken cancellationToken)
    {
        return _queue.Writer.WriteAsync(item, cancellationToken);
    }

    public ValueTask<TransactionWorkItem> DequeueAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}