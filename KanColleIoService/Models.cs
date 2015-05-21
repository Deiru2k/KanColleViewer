using System;

namespace KanColleIoService.Models
{
    internal class Response<T>
    {
        public int code { get; set; }
        public string status { get; set; }
        public T data { get; set; }
    }

    internal class Ship
    {
        public object id { get; set; }
        public string origin { get; set; }
        public int baseId { get; set; }
        public int level { get; set; }
        public int[] equipment { get; set; }
        public Stats stats { get; set; }
    }

    internal class Stats
    {
        public int hp { get; set; }
        public int firepower { get; set; }
        public int armor { get; set; }
        public int torpedo { get; set; }
        public int evasion { get; set; }
        public int aa { get; set; }
        public int aircraft { get; set; }
        public int asw { get; set; }
        public string speed { get; set; }
        public int los { get; set; }
        public string range { get; set; }
        public int luck { get; set; }
    }
}
