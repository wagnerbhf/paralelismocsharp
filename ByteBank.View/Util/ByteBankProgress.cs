﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace ByteBank.View.Util
{
    public class ByteBankProgress<T> : IProgress<T>
    {
        private readonly Action<T> _handler;
        private readonly TaskScheduler _taskScheduler;

        public ByteBankProgress(Action<T> handler)
        {
            _handler = handler;
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }

        public void Report(T value)
        {
            Task.Factory.StartNew(() => _handler(value), CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }
    }
}