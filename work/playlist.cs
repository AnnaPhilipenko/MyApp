//------------------------------------------------------------------------------
// <auto-generated>
//    Этот код был создан из шаблона.
//
//    Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//    Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace work
{
    using System;
    using System.Collections.Generic;
    
    public partial class playlist
    {
        public playlist()
        {
            this.songs = new HashSet<songs>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
    
        public virtual ICollection<songs> songs { get; set; }
    }
}
