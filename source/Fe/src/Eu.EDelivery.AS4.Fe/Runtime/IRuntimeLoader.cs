﻿using Eu.EDelivery.AS4.Fe.Services;
using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    public interface IRuntimeLoader : IModular
    {
        IEnumerable<ItemType> Receivers { get; }
        IEnumerable<ItemType> Steps { get; }
        IEnumerable<ItemType> Transformers { get; }
        IEnumerable<ItemType> CertificateRepositories { get; }
        IEnumerable<ItemType> DeliverSenders { get; }
        IEnumerable<ItemType> ReceivingPmode { get; }
        IRuntimeLoader Initialize();
    }
}