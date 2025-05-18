[System.Serializable]
public class ProcessDefinition
{
    public string processName;
    public float arrivalTime;
    public float burstTime;
    public float priority;
}

[System.Serializable]
public class ProcessDefinitionList
{
    public ProcessDefinition[] processes;
}