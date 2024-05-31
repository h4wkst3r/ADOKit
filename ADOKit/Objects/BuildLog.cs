namespace ADOKit.Objects
{
    class BuildLog
    {

        public int buildID { get; set; }
        public int logID { get; set; }
        public string logURL { get; set; }
        public int lineCount { get; set; }

        public BuildLog(int buildID, int logID, string logURL, int lineCount)
        {
            this.buildID = buildID;
            this.logID = logID;
            this.logURL = logURL;
            this.lineCount = lineCount;
        }


    }
}
