namespace MES.Domain.Enums;

public enum SerialStatus
{
    Created = 0,
    InProcess = 1,
    TestedPass = 2,
    TestedFail = 3,
    Rework = 4,
    Scrapped = 5,
    Done = 6
}
