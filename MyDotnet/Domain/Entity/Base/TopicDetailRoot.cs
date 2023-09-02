using System;

namespace MyDotnet.Domain.Entity.Base
{
    /// <summary>
    /// Tibug 博文
    /// </summary>
    public class TopicDetailRoot<Tkey> : RootEntityTkey<Tkey> where Tkey : IEquatable<Tkey>
    {
        public Tkey TopicId { get; set; }
    }
}
