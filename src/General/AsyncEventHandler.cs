using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lili.Protocol.General;

public delegate Task AsyncEventHandler(object sender, EventArgs e, CancellationToken cancellationToken = default);

public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e, CancellationToken cancellationToken = default);