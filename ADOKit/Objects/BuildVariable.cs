namespace ADOKit.Objects
{
    class BuildVariable
    {

        public string name { get; set; }
        public string value { get; set; }
        public string pipelineName { get; set; }

        public BuildVariable(string name, string value, string pipelineName)
        {
            this.name = name;
            this.value = value;
            this.pipelineName = pipelineName;
        }


    }
}
