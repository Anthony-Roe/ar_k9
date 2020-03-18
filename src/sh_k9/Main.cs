namespace sh_k9
{
    using System.Collections.Generic;

    public class Settings
    {
        public bool standalone { get; set; }
        public string allowedJobGrade { get; set; }
        public string vehicleBoneToAttachTo { get; set; }
        public List<string> illegalItems { get; set; }
        public List<string> illegalWeapons { get; set; }
        public IDictionary<string, dynamic> dict { get; set; }
        public IDictionary<string, dynamic> dog { get; set; }
    }
}
