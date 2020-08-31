﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NewLife.Collections;
using NewLife.Data;
using Xunit;

namespace XUnitTest.Data
{
    public class FlowIdTests
    {
        [Fact]
        public void NewId()
        {
            var fid = new FlowId();
            var id = fid.NewId();

            var time = id >> 22;
            Assert.True(fid.StartTimestamp.AddMilliseconds(time) <= DateTime.Now);

            var wid = (id >> 12) & 0x3FF;
            Assert.Equal(fid.WorkerId, wid);

            var seq = id & 0x0FFF;
            Assert.Equal(fid.Sequence, seq);
        }

        [Fact]
        public void ValidRepeat()
        {
            var sw = Stopwatch.StartNew();

            var ws = new ConcurrentBag<Int32>();
            var repeat = new ConcurrentBag<Int64>();
            var hash = new ConcurrentHashSet<Int64>();

            var ts = new List<Task>();
            for (var k = 0; k < 10; k++)
            {
                ts.Add(Task.Run(() =>
                {
                    var f = new FlowId { StartTimestamp = new DateTime(2020, 1, 1) };
                    ws.Add(f.WorkerId);

                    for (var i = 0; i < 1_000_000; i++)
                    {
                        var id = f.NewId();
                        if (!hash.TryAdd(id)) repeat.Add(id);
                    }
                }));
            }
            Task.WaitAll(ts.ToArray());

            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 10_000);
            var count = repeat.Count;
            Assert.Equal(0, count);
        }

        [Fact]
        public void Benchmark()
        {
            var f = new FlowId();

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < 10_000_000; i++)
            {
                var id = f.NewId();
            }

            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 10_000);
        }
    }
}