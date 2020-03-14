namespace sh_k9
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    public class Settings
    {
        public List<string> illegalItems { get; set; }
        public List<string> illegalWeapons { get; set; }
        public string allowedJobGrade { get; set; }
    }
}
