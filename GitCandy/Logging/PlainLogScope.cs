using System;
using System.Threading;

namespace GitCandy.Logging
{
    // Microsoft.Extensions.Logging.Console.ConsoleLogScope.cs
    // Copyright (c) .NET Foundation. All rights reserved.
    // Modified by Aimeast.
    // Licensed under the Apache License, Version 2.0.

    public class PlainLogScope
    {
        private readonly string _name;
        private readonly object _state;

        internal PlainLogScope(string name, object state)
        {
            _name = name;
            _state = state;
        }

        public PlainLogScope Parent { get; private set; }

        private static AsyncLocal<PlainLogScope> _value = new AsyncLocal<PlainLogScope>();

        public static PlainLogScope Current
        {
            set
            {
                _value.Value = value;
            }
            get
            {
                return _value.Value;
            }
        }

        public static IDisposable Push(string name, object state)
        {
            var temp = Current;
            Current = new PlainLogScope(name, state);
            Current.Parent = temp;

            return new DisposableScope();
        }

        public override string ToString()
        {
            return _state?.ToString();
        }

        private class DisposableScope : IDisposable
        {
            public void Dispose()
            {
                Current = Current.Parent;
            }
        }
    }
}
