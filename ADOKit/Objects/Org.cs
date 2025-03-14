namespace ADOKit.Objects
{
    class Org
    {

        public string orgName { get; set; }
        public string orgID { get; set; }
        public string url { get; set; }
        public string owner { get; set; }

        public Org(string orgName, string orgID, string url = "", string owner = "")
        {
            this.orgName = orgName;
            this.orgID = orgID;
            this.url = url;
            this.owner = owner;
        }
    }
}
