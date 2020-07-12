using System.ComponentModel;

namespace DominosLocationMap.Entities.Consts
{
    public static class RabbitMqConsts
    {
        /// yaşam süresi
        public static int MessagesTTL { get; set; } = 1000 * 60 * 60 * 2;

        //Aynı anda - Eşzamanlı e-posta gönderimi sayısı, thread açma için sınırı belirleriz
        public static ushort ParallelThreadsCount { get; set; } = 15;

        public enum RabbitMqConstsList
        {
            [Description("DominosLocationDatabaseQueue")]
            DominosLocationDatabaseQueue = 1,

            [Description("DominosLocationFileOptionQueue")]
            DominosLocationFileOptionQueue = 2,
        }
    }
}