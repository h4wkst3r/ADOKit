namespace ADOKit.Objects
{
    class Build
    {

        public int ID { get; set; }
        public string URL { get; set; }
        public string buildDefinitionName { get; set; }

        public Build(int ID, string URL, string buildDefinitionName)
        {
            this.ID = ID;
            this.URL = URL;
            this.buildDefinitionName = buildDefinitionName;
        }


    }
}
