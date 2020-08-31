﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NewLife.Security;

namespace NewLife.Data
{
    /// <summary>雪花算法。分布式Id</summary>
    /// <remarks>
    /// 使用一个 64 bit 的 long 型的数字作为全局唯一 id。在分布式系统中的应用十分广泛，且ID 引入了时间戳，基本上保持自增。
    /// 1bit保留 + 41bit时间戳 + 10bit机器 + 12bit序列号
    /// </remarks>
    public class FlowId
    {
        #region 属性
        /// <summary>开始时间戳。默认1970-1-1</summary>
        public DateTime StartTimestamp { get; set; } = new DateTime(1970, 1, 1);

        /// <summary>机器Id，取10位</summary>
        public Int32 WorkerId { get; set; }

        private Int32 _Sequence;
        /// <summary>序列号，取12位</summary>
        public Int32 Sequence { get => _Sequence; set => _Sequence = value; }

        private Int64 _lastTime;
        #endregion

        #region 构造
        /// <summary>实例化</summary>
        public FlowId()
        {
            WorkerId = Rand.Next() & 0x3FF;
        }
        #endregion

        #region 核心方法
        /// <summary>获取下一个Id</summary>
        /// <returns></returns>
        public virtual Int64 NewId()
        {
            var time = (Int64)(DateTime.Now - StartTimestamp).TotalMilliseconds;
            var nid = WorkerId & 0x3FF;
            var seq = Interlocked.Increment(ref _Sequence) & 0x0FFF;

            // 相同毫秒内，如果序列号用尽，则可能超过4096，导致生成重复Id
            // 睡眠1毫秒，抢占它的位置 @656092719（广西-风吹面）
            if (_lastTime == time && seq == 0)
            {
                time++;
                Thread.Sleep(1);
            }
            _lastTime = time;

            return (time << (10 + 12)) | (Int64)(nid << 12) | (Int64)seq;
        }
        #endregion
    }
}