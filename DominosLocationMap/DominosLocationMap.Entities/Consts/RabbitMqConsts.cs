using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DominosLocationMap.Entities.Consts
{
    public static class RabbitMqConsts
    {

        /// yaşam süresi
        public static int MessagesTTL { get; set; } = 1000 * 60 * 60 * 2;

        //Aynı anda - Eşzamanlı e-posta gönderimi sayısı, thread açma için sınırı belirleriz
        public static ushort ParallelThreadsCount { get; set; } = 3;
        public enum RabbitMqConstsList
        {
            [Description("QueueNameLocation")]
            QueueNameLocation = 1,
            [Description("QueueNameTextWrite")]
            QueueNameTextWrite = 2,
            [Description("DominosLocationReadDataQueue")]
            DominosLocationReadDataQueue = 3
        }

    }
}
